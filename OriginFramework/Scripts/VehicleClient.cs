using CitizenFX.Core;
using MenuAPI;
using Newtonsoft.Json;
using OriginFramework.Menus;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using static OriginFramework.OfwFunctions;


namespace OriginFramework
{
  public class VehicleClient : BaseScript
  {
    private class PolyTest
    {
      public PolyTest(Vector3 vec, float h)
      {
        Vec = vec;
        Heading = h;
      }

      public Vector3 Vec { get; set; }
      public float Heading { get; set; }
    }

    private List<PolyTest> Tests = new List<PolyTest>();
    private PolyTest LastTest = null; 
    private bool testsFrozen = false;

    public VehicleClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
      EventHandlers["ofw_veh:UpdateSavedVehicleProperties"] += new Action<int, string>(UpdateSavedVehicleProperties);
    }


    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      RegisterCommand("tpoly", new Action<int, List<object>, string>(async (source, args, raw) =>
      {
        testsFrozen = true;

        if (args == null || args.Count != 4)
          Tests.Clear();
        else
          Tests.Add(new PolyTest(new Vector3(float.Parse(args[0].ToString()), float.Parse(args[1].ToString()), float.Parse(args[2].ToString())), float.Parse(args[3].ToString())));

        testsFrozen = false;
      }), false);

      Tick += DoShapetests;
    }

    public async Task DoShapetests()
    {
      if (Tests.Count <= 0)
      {
        await Delay(1000);
        return;
      }

      foreach (var i in Tests)
      {
        if (testsFrozen)
          break;

        var blocked = GetParkingSpotBlockingEntity(i.Vec, i.Heading) > 0;
        ShapeBoxDraw(i.Vec, i.Heading, blocked);
      }
    }

    private async void OnResourceStop(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;


    }

    public static async void UpdateSavedVehicleProperties(int vehNetId, string plate)
    {
      var localVeh = NetworkGetEntityFromNetworkId(vehNetId);
      var veh = -1;
      float timeout = 5000;

      while (timeout > 0 && veh <= 0)
      {
        veh = GetVehiclePedIsUsing(Game.PlayerPed.Handle);
        await Delay(0);
        timeout -= (GetFrameTime() * 1000);
      }

      if (veh <= 0 || veh != localVeh)
      {
        Notify.Error("Nepodarilo se ulozit vlastnosti vozidla, pred pouzivanim se prosim stav v garazi");
        return;
      }

      var vehicleProps = GetVehicleProperties(veh);

      Console.WriteLine(JsonConvert.SerializeObject(vehicleProps));

      TriggerServerEvent("ofw_esxgarage:UpdateVehicleProperties", plate, JsonConvert.SerializeObject(vehicleProps));
    }

    private Vector3 GetAngledPosition(Vector3 center, float dist, float angle, float mod)
    {
      double angRad = (Math.PI / 180) * angle;
      return center + mod * dist * new Vector3((float)Math.Cos(angRad), (float)Math.Sin(angRad), 0.0f);
    }

    private void DrawPolyOfw(Vector3 v1, Vector3 v2, Vector3 v3, int r, int g, int b, int a)
    {
      DrawPoly(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z, r, g, b, a);
    }

    private void ShapeBoxDraw(Vector3 center, float heading, bool blocked)
    {
      ShapeBoxDraw(center, heading, 2.3f, 4.5f, 2.0f, blocked);
    }

    private void ShapeBoxDraw(Vector3 center, float heading, float width, float length, float height, bool blocked)
    {
      float diagonal = (float)Math.Sqrt(Math.Pow(width / 2f, 2) + Math.Pow(length / 2f, 2));
      float fullDiagonal = (float)Math.Sqrt(Math.Pow(width, 2) + Math.Pow(length, 2));
      var boxHeight = new Vector3(0.0f, 0.0f, height);

      float newAngle = (float)((Math.Asin(length / fullDiagonal) * 180) / Math.PI);

      var topRight = GetAngledPosition(center, diagonal, heading + newAngle, 1);
      var bottomRight = GetAngledPosition(center, diagonal, heading - newAngle, 1);
      var bottomLeft = GetAngledPosition(center, diagonal, heading + newAngle, -1);
      var topLeft = GetAngledPosition(center, diagonal, heading - newAngle, -1);

      var off = new Vector3(0.0f, 0.0f, 5.0f);

      var boxColor = new int[] { 0, 255, 0 };

      if (blocked)
        boxColor = new int[] { 255, 0, 0 };

      Vector3 v1 = Vector3.Subtract(topRight, off);
      Vector3 v2 = Vector3.Add(topRight, off);
      DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      v1 = Vector3.Subtract(bottomRight, off);
      v2 = Vector3.Add(bottomRight, off);
      DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      v1 = Vector3.Subtract(bottomLeft, off);
      v2 = Vector3.Add(bottomLeft, off);
      DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

      v1 = Vector3.Subtract(topLeft, off);
      v2 = Vector3.Add(topLeft, off);
      DrawLine(v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, 0, 255, 0, 255);

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
  }
}
