using CitizenFX.Core;
using CitizenFX.Core.Native;
using OriginFramework.Helpers;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.UI.Screen;

namespace OriginFramework
{
  public static class OfwFunctions
  {
    #region box markers
    public static int GetBoxMarkerBlockingVehicle(PosBag center, DimensionsBag dimensions)
    {
      return GetBoxMarkerBlockingEntity(center, dimensions, 2);
    }

    public static int GetBoxMarkerBlockingEntity(PosBag center, DimensionsBag dimensions, int flags)
    {
      //--2->auta
      //-- 4->peds
      //-- 16->objekty
      //-- 256->rostliny
      int ray = StartShapeTestBox(
        center.X, center.Y, center.Z,
        dimensions.Width, dimensions.Length, dimensions.Height,
        0.0f, 0.0f, center.Heading, 2,
        flags,
        0, //entita, kterou má raycast ignorovat(např.PlayerPedId() pokud chceme ignorovat sveho peda)
        4); //neznamy parametr

      bool hit = false;
      Vector3 endCoords = new Vector3();
      Vector3 surfaceNormal = new Vector3();
      int entityHit = 0;

      GetShapeTestResult(ray, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);

      return entityHit;
    }

    public static void BoxMarkerSolidDraw(PosBag center, DimensionsBag dimensions, bool blocked)
    {
      float diagonal = (float)Math.Sqrt(Math.Pow(dimensions.Width / 2f, 2) + Math.Pow(dimensions.Length / 2f, 2));
      float fullDiagonal = (float)Math.Sqrt(Math.Pow(dimensions.Width, 2) + Math.Pow(dimensions.Length, 2));
      var boxHeight = new Vector3(0.0f, 0.0f, dimensions.Height);

      float newAngle = (float)((Math.Asin(dimensions.Length / fullDiagonal) * 180) / Math.PI);

      var topRight = GetAngledPosition(center, diagonal, center.Heading + newAngle, 1);
      var bottomRight = GetAngledPosition(center, diagonal, center.Heading - newAngle, 1);
      var bottomLeft = GetAngledPosition(center, diagonal, center.Heading + newAngle, -1);
      var topLeft = GetAngledPosition(center, diagonal, center.Heading - newAngle, -1);

      //var off = new Vector3(0.0f, 0.0f, 5.0f);

      var boxColor = new int[] { 0, 255, 0 };

      if (blocked)
        boxColor = new int[] { 255, 0, 0 };

      //Vector3 v1 = Vector3.Subtract(topRight, off);
      //Vector3 v2 = Vector3.Add(topRight, off);
      //DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      //v1 = Vector3.Subtract(bottomRight, off);
      //v2 = Vector3.Add(bottomRight, off);
      //DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      //v1 = Vector3.Subtract(bottomLeft, off);
      //v2 = Vector3.Add(bottomLeft, off);
      //DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      //v1 = Vector3.Subtract(topLeft, off);
      //v2 = Vector3.Add(topLeft, off);
      //DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      DrawPolyOfw(topRight, topLeft, topLeft + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
      DrawPolyOfw(topRight, topLeft + boxHeight, topRight + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);


      DrawPolyOfw(bottomLeft, bottomRight, bottomRight + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
      DrawPolyOfw(bottomLeft, bottomRight + boxHeight, bottomLeft + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);


      DrawPolyOfw(topLeft, bottomLeft, bottomLeft + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
      DrawPolyOfw(topLeft, bottomLeft + boxHeight, topLeft + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);


      DrawPolyOfw(bottomRight, topRight, topRight + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
      DrawPolyOfw(bottomRight, topRight + boxHeight, bottomRight + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);


      DrawPolyOfw(bottomRight + boxHeight, topRight + boxHeight, topLeft + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
      DrawPolyOfw(topLeft + boxHeight, bottomLeft + boxHeight, bottomRight + boxHeight, boxColor[0], boxColor[1], boxColor[2], 100);
    }

    private static Vector3 GetAngledPosition(PosBag center, float dist, float angle, float mod)
    {
      double angRad = (Math.PI / 180) * angle;
      var newVect = mod * dist * new Vector3((float)Math.Cos(angRad), (float)Math.Sin(angRad), 0.0f);
      return newVect + PosBagToVector3(center);
    }

    private static void DrawPolyOfw(Vector3 v1, Vector3 v2, Vector3 v3, int r, int g, int b, int a)
    {
      DrawPoly(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z, r, g, b, a);
    }

    public static void DrawBoxMarker(PosBag pos, DimensionsBag dim, ColorBag c)
    {
      DrawMarker(43, pos.X, pos.Y, pos.Z - 0.2f, 0f, 0f, 0f, 0f, 0f, pos.Heading,
                 dim.Width, dim.Length, dim.Height / 3f, //jen tretinova vyska
                 c.R, c.G, c.B, c.A,
                 false, false, 2, false, null, null, false);
    }

    public static bool IsInBoxMarkerFast(Vector3 playerPos, PosBag targetPos, DimensionsBag targetDim)
    {
      var targetVec = PosBagToVector3(targetPos);
      float avgDist = (targetDim.Length + targetDim.Width) / 4;
      return playerPos.DistanceToSquared2D(targetVec) <= avgDist * avgDist;
    }

    public static bool IsPointInBoxMarker(Vector3 point, PosBag pos, DimensionsBag dim)
    {
      var polygonList = GetBoxMarkerPolygon(pos, dim);

      return IsInsidePolygon(polygonList, point);
    }

    public static bool IsVehicleInBoxMarker(int veh, PosBag pos, DimensionsBag dim)
    {
      int trailerVeh = 0;
      bool hasTrailer = GetVehicleTrailerVehicle(veh, ref trailerVeh);

      int boneLR = GetEntityBoneIndexByName(hasTrailer ? trailerVeh : veh, "wheel_lr");
      int boneRR = GetEntityBoneIndexByName(hasTrailer ? trailerVeh : veh, "wheel_rr");
      int boneRF = GetEntityBoneIndexByName(veh, "wheel_rf");
      int boneLF = GetEntityBoneIndexByName(veh, "wheel_lf");

      Vector3 boneLRCoords = GetWorldPositionOfEntityBone(hasTrailer ? trailerVeh : veh, boneLR);
      Vector3 boneRRCoords = GetWorldPositionOfEntityBone(hasTrailer ? trailerVeh : veh, boneRR);
      Vector3 boneRFCoords = GetWorldPositionOfEntityBone(veh, boneRF);
      Vector3 boneLFCoords = GetWorldPositionOfEntityBone(veh, boneLF);

      var polygonList = GetBoxMarkerPolygon(pos, dim);

      return IsInsidePolygon(polygonList, boneLRCoords) && (boneRR == -1 || IsInsidePolygon(polygonList, boneRRCoords)) && (boneRF == -1 || IsInsidePolygon(polygonList, boneRFCoords)) && IsInsidePolygon(polygonList, boneLFCoords);
    }

    private static List<Vector3> GetBoxMarkerPolygon(PosBag pos, DimensionsBag dim)
    {
      //Tohle cely by se teoreticky dalo cashovat...
      Vector3 baseVector = PosBagToVector3(pos);

      float x = dim.Width / 2;
      float y = dim.Length / 2;

      var headingRad = MathUtil.DegreesToRadians(pos.Heading);

      Vector3 topLeft = baseVector + RotateVector(new Vector3(x, y, 0f), headingRad);
      Vector3 topRight = baseVector + RotateVector(new Vector3(x, -y, 0f), headingRad);
      Vector3 bottomLeft = baseVector + RotateVector(new Vector3(-x, -y, 0f), headingRad);
      Vector3 bottomRight = baseVector + RotateVector(new Vector3(-x, y, 0f), headingRad);

      if (TheBugger.DebugMode && false)
      {
        DrawBox(
        topLeft.X - 0.05f, topLeft.Y - 0.05f, topLeft.Z - 5.05f,
        topLeft.X + 0.05f, topLeft.Y + 0.05f, topLeft.Z + 5.05f,
        255, 0, 0, 200
        );
        DrawBox(
        topRight.X - 0.05f, topRight.Y - 0.05f, topRight.Z - 5.05f,
        topRight.X + 0.05f, topRight.Y + 0.05f, topRight.Z + 5.05f,
        255, 0, 0, 200
        );
        DrawBox(
        bottomLeft.X - 0.05f, bottomLeft.Y - 0.05f, bottomLeft.Z - 5.05f,
        bottomLeft.X + 0.05f, bottomLeft.Y + 0.05f, bottomLeft.Z + 5.05f,
        255, 0, 0, 200
        );
        DrawBox(
        bottomRight.X - 0.05f, bottomRight.Y - 0.05f, bottomRight.Z - 5.05f,
        bottomRight.X + 0.05f, bottomRight.Y + 0.05f, bottomRight.Z + 5.05f,
        255, 0, 0, 200
        );
      }

      return new List<Vector3>() { topLeft, topRight, bottomLeft, bottomRight };
    }

    private static Vector3 RotateVector(Vector3 v, float HeadingRad)
    {
      float cos = (float)Math.Cos(HeadingRad);
      float sin = (float)Math.Sin(HeadingRad);
      return new Vector3(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos, v.Z);
    }

    private static bool IsInsidePolygon(List<Vector3> polygon, Vector3 point)
    {
      bool isInside = false;
      int polygonLength = polygon.Count;
      int i = 0, j = polygonLength - 1;

      for (; i < polygonLength; i++)
      {
        if ((polygon[i].Y < point.Y && polygon[j].Y >= point.Y
            || polygon[j].Y < point.Y && polygon[i].Y >= point.Y)
            && (polygon[i].X <= point.X || polygon[j].X <= point.X))
        {
          isInside ^= (polygon[i].X + (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y)
                       * (polygon[j].X - polygon[i].X) < point.X);
        }
        j = i;
      }

      return isInside;
    }
    #endregion

    public static async Task<bool> MovePedToCoordForSeconds(int ped, Vector3 pos, float heading, float slideDist, float moveForSeconds)
    {
      Function.Call(Hash.TASK_GO_STRAIGHT_TO_COORD, ped, pos.X, pos.Y, pos.Z + 0.5f, 1.0f, -1, heading, slideDist);
      TaskGoStraightToCoord(ped, pos.X, pos.Y, pos.Z + 0.5f, 1.0f, -1, heading, slideDist);

      while (true)
      {
        await BaseScript.Delay(0);
        moveForSeconds -= Game.LastFrameTime;
        Vector3 newCoords = GetEntityCoords(ped, false);
        float newHeading = GetEntityHeading(ped);

        float coordsDiff = Vector2.Distance(new Vector2(newCoords.X, newCoords.Y), new Vector2(pos.X, pos.Y));
        float headingDiff = Math.Abs(newHeading - heading);

        if (coordsDiff < 0.1f && headingDiff < 5.0f || moveForSeconds < 0.0f)
        {
          await BaseScript.Delay(100);
          break;
        }
      }

      return true;
    }

    public static Vector3 PosBagToVector3(PosBag posBag)
    {
      return new Vector3(posBag.X, posBag.Y, posBag.Z);
    }
  }
}
