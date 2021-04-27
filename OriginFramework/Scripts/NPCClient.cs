using CitizenFX.Core;
using Newtonsoft.Json;
using OriginFrameworkData.DataBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;


namespace OriginFramework
{
  public class NPCClient : BaseScript
  {
    public static NPCDefinitionBag[] NPCs { get; private set; } = null;
    private Dictionary<int, uint> DictionaryGroupPGroupGTA = new Dictionary<int, uint>();

    public static Control NPCInteractionKey { get; private set; } = Control.Context;
    private string NPCInteractionKeyString = " ~INPUT_CONTEXT~ ";

    public NPCClient()
    {
      EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
      EventHandlers["ofw_npc:UpdateNPCNetID"] += new Action<string, int>(UpdateNPCNetID);
    }

    private async void OnClientResourceStart(string resourceName)
    {
      if (CitizenFX.Core.Native.API.GetCurrentResourceName() != resourceName) return;

      while (SettingsManager.Settings == null)
        await Delay(0);

      AddTextEntry("OFW_NPC_INTERACT", $"Stiskni {NPCInteractionKeyString} pro interakci");

      string ret = null;
      bool completed = false;

      Func<string, bool> CallbackFunction = (data) =>
      {
        ret = data;
        completed = true;
        return true;
      };

      BaseScript.TriggerServerEvent("ofw_npc:GetAllNPC", CallbackFunction);

      while (!completed)
      {
        await Delay(0);
      }

      if (ret != null)
        NPCs = JsonConvert.DeserializeObject<NPCDefinitionBag[]>(ret);

      Tick += NPCSlowTick;
      Tick += NPCFastTick;
    }

    private async Task NPCSlowTick()
		{
      if (NPCs == null)
      {
        await Delay(5000);
        return;
      }

      foreach (var i in NPCs)
      {
        if (i.SpawnedNetID <= 0)
          continue;

        if (!NetworkDoesEntityExistWithNetworkId(i.SpawnedNetID))
          continue;

        var pid = NetworkGetEntityFromNetworkId(i.SpawnedNetID);

        if (pid <= 0)
          continue; 

        var ped = new Ped(pid);
        SetPedDropsWeaponsWhenDead(pid, i.CanDropWeapon);
        SetPedCombatAttributes(pid, 5, true);
        SetPedCombatAttributes(pid, 46, true);
        if (GetSelectedPedWeapon(pid) != GetHashKey(i.WeaponModelName))
        {
          GiveWeaponToPed(pid, (uint)GetHashKey(i.WeaponModelName), 999999, false, true);
          SetCurrentPedWeapon(pid, (uint)GetHashKey(i.WeaponModelName), true);
        }

        if (i.HasNoColissions)
        {
          ped.IsCollisionEnabled = false;
          ped.IsCollisionProof = true;
          ClearPedTasks(pid);
        }

        ped.IsPositionFrozen = i.IsPositionFrozen;
        ped.IsInvincible = i.IsInvincible;

        if (i.Group > 0)
        {
          uint gtaGroup = 0;
          if (DictionaryGroupPGroupGTA.ContainsKey(i.Group))
          {
            gtaGroup = DictionaryGroupPGroupGTA[i.Group];
          }
          else
          {
            AddRelationshipGroup(i.Group.ToString(), ref gtaGroup);
            Debug.WriteLine("CreatedGroup " + gtaGroup);
            if (gtaGroup > 0)
              DictionaryGroupPGroupGTA.Add(i.Group, gtaGroup);
          }
          
          if (gtaGroup > 0 && DoesRelationshipGroupExist((int)gtaGroup))
          {
            if (GetPedRelationshipGroupHash(pid) != (int)gtaGroup)
            {
              SetPedRelationshipGroupHash(pid, gtaGroup);
            }
          }
        }
      }

      await Delay(1000);
    }

    private async Task NPCFastTick()
    {
      if (NPCs == null)
      {
        await Delay(5000);
        return;
      }

      var playerPos = Game.PlayerPed.Position;

      foreach (var i in NPCs)
      {
        if (i.SpawnedNetID <= 0)
          continue;

        if (!NetworkDoesEntityExistWithNetworkId(i.SpawnedNetID))
          continue;

        var pid = NetworkGetEntityFromNetworkId(i.SpawnedNetID);

        if (pid <= 0)
          continue;

        var ped = new Ped(pid);

        var distance = Vector3.Distance(ped.Position, playerPos);
        if (distance < 50 && ped.IsOnScreen)
        {
          if (!string.IsNullOrEmpty(i.VisibleName) && HasEntityClearLosToEntity(Game.PlayerPed.Handle, pid, 17)) //Hoodne draha operace, co na to vykon scriptu?
            DrawNpcName(ped.Position, distance, i.VisibleName);
        }

        if (distance < 2 && i.OnInteraction != null)
        {
          DisplayHelpTextThisFrame("OFW_NPC_INTERACT", false);
          if (IsControlJustPressed(0, (int)NPCInteractionKey))
          {
            i.OnInteraction(pid, i.VisibleName);
          }
        }
      }
    }

    private async void UpdateNPCNetID(string npcName, int netID)
    {
      if (NPCs == null)
        await Delay(100);

      foreach (var i in NPCs)
      {
        if (i.UniqueName == npcName)
          i.SpawnedNetID = netID;
      }
    }

    public static bool IsNpcInRange(NPCDefinitionBag npc, Vector3 ppos)
    {
      if (!NetworkDoesEntityExistWithNetworkId(npc.SpawnedNetID))
        return false;

      var pid = NetworkGetEntityFromNetworkId(npc.SpawnedNetID);

      if (pid <= 0)
        return false;

      var ped = new Ped(pid);

      var dist = Vector3.Distance(ped.Position, ppos);
      Debug.WriteLine($"Dist: {dist}");
      if (dist <= 2)
        return true;
      else
        return false;
    }

    private void DrawNpcName(Vector3 pos, float distance, string name)
    {
      if (distance < 5f)
        distance = 5f;

      const float perspectiveScale = 1.8f;
      float _x = 0, _y = 0;
      World3dToScreen2d(pos.X, pos.Y, pos.Z + 1f, ref _x, ref _y);
      var p = GetGameplayCamCoords();
      //var fov = (1 / GetGameplayCamFov()) * 75;
      var scale = ((1 / distance) * perspectiveScale) /* * fov*/;

      SetTextScale(1, scale);
      SetTextFont(0);
      SetTextProportional(true);
      SetTextColour(255, 122, 0, 255);
      SetTextOutline();
      SetTextEntry("STRING");
      SetTextCentre(true);
      AddTextComponentString(name);
      DrawText(_x, _y);
    }
  }
}
