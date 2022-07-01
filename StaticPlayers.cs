using System;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using System.IO;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Static Players", "Chinese Rust Server Owner", "0.0.1")]
    [Description("Create Static Players")]
    class StaticPlayers : RustPlugin
    {
        bool init = false;
        const string playerPrefab = "assets/prefabs/player/player.prefab";
        private const string permissionName = "StaticPlayers.use";

        List<object> DefaultShirts() => new List<object> { "tshirt", "tshirt.long", "shirt.tanktop", "shirt.collared" };
        List<object> DefaultPants() => new List<object> { "pants" };
        List<object> DefaultHelms() => new List<object> { "metal.facemask" };
        List<object> DefaultVests() => new List<object> { "metal.plate.torso" };
        List<object> DefaultGloves() => new List<object> { "burlap.gloves" };
        List<object> DefaultBoots() => new List<object> { "shoes.boots" };
        List<object> DefaultWeapons() => new List<object> { "pistol.semiauto" };

        static List<uint> awakers = new List<uint>();

        class Fake : MonoBehaviour
        {
            BasePlayer clone;
            BasePlayer fake;
            List<Item> items;
            uint cloneID;

            void Awake()
            {
                fake = GetComponent<BasePlayer>();
                fake.net.connection.connected = true;
                fake.net.connection.connectionTime = 0;
                fake.net.connection.ipaddress = "0.0.0.0";
                fake.violationLevel = 0f;
                fake.lastViolationTime = 0f;
                fake.lastAdminCheatTime = 0f;
                fake.speedhackPauseTime = 0f;
                fake.speedhackDistance = 0f;
                fake.flyhackPauseTime = 0f;
                fake.flyhackDistanceVertical = 0f;
                fake.flyhackDistanceHorizontal = 0f;
                fake.rpcHistory.Clear();
                fake.health = 100f;

                items = new List<Item>();

                fake.inventory.DoDestroy();
                fake.inventory.ServerInit(fake);
                fake.inventory.ServerUpdate(0f);
                fake.SetPlayerFlag(BasePlayer.PlayerFlags.Connected, true);

                fake.displayName = "StaticPlayer";

                fake.SendNetworkUpdateImmediate(true);
                Equip();

                if (clone != null)
                {
                    if (!awakers.Contains(clone.net.ID))
                    {
                        awakers.Add(clone.net.ID);
                        cloneID = clone.net.ID;
                    }
                }
            }

            void OnDestroy()
            {
                if (awakers.Contains(cloneID))
                {
                    awakers.Remove(cloneID);
                }

                if (stripFakes)
                    foreach (Item item in items)
                        item?.Remove(0.01f);

                if (fake != null && !fake.IsDestroyed)
                    fake.Kill();

                items.Clear();
                Destroy(this);
            }

            void Equip()
            {
                if (gloves.Count > 0)
                {
                    Item item = ItemManager.CreateByName(gloves.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (boots.Count > 0)
                {
                    Item item = ItemManager.CreateByName(boots.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (helms.Count > 0)
                {
                    Item item = ItemManager.CreateByName(helms.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (vests.Count > 0)
                {
                    Item item = ItemManager.CreateByName(vests.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (shirts.Count > 0)
                {
                    Item item = ItemManager.CreateByName(shirts.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (pants.Count > 0)
                {
                    Item item = ItemManager.CreateByName(pants.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);
                    item.MarkDirty();

                    item.MoveToContainer(fake.inventory.containerWear, -1, false);
                    items.Add(item);
                }

                if (weapons.Count > 0)
                {
                    Item item = ItemManager.CreateByName(weapons.GetRandom());
                    item.skin = Convert.ToUInt64(item.info.skins.GetRandom().id);

                    if (item.skin != 0 && item.GetHeldEntity())
                        item.GetHeldEntity().skinID = item.skin;

                    item.MarkDirty();
                    item.MoveToContainer(fake.inventory.containerBelt, -1, false);
                    items.Add(item);
                }
            }
        }
        [ConsoleCommand("createstaticplayer")]
        void ccmdCreateFake(ConsoleSystem.Arg arg)
        {
            if (!init)
                return;
            var fake = new BasePlayer();
            if(arg.Player() == null)
            {
                Vector3 v = new Vector3(0,0,0);
                Quaternion q = Quaternion.Euler(90, 0, 0);
                fake = GameManager.server.CreateEntity(playerPrefab, v, q, true) as BasePlayer;
            }
            else
            {
                var player = arg.Player();
                if (!arg.IsAdmin && !permission.UserHasPermission(player.userID.ToString(), permissionName))
                {
                    player.ChatMessage(string.Format("您没有执行此操作的权限，权限：{0}", permissionName));
                    return;
                }

                RaycastHit hit;
                if (!Physics.Raycast(player.eyes.HeadRay(), out hit, maxDistance))
                {
                    player.ChatMessage(msg("执行失败", player.UserIDString));
                    return;
                }
                fake = GameManager.server.CreateEntity(playerPrefab, hit.point, player.transform.rotation, true) as BasePlayer;
            }
            ulong fakeId = RandomFakeID();
            fake.userID = fakeId;
            fake.UserIDString = fakeId.ToString();
            fake.displayName = "StaticPlayer" + fakeId.ToString();
            fake.Spawn();
            global::BasePlayer.activePlayerList.Add(fake);
        }

        ulong RandomFakeID()
        {
            System.Random r = new System.Random();
            string id = r.Next(100000, 999999).ToString();
            id = id.Substring(0,3) + "000" + id.Substring(3,3);
            return ulong.Parse(id);
        }

        void OnServerInitialized()
        {
            LoadVariables();

            if (!destroyFakeCorpses)
                Unsubscribe(nameof(OnEntitySpawned));

            if (!preventFakeLooting)
                Unsubscribe(nameof(OnLootEntity));

            if (!hideAwakers)
                Unsubscribe(nameof(CanNetworkTo));

            init = true;

            permission.RegisterPermission(permissionName, this);
        }

        void OnEntitySpawned(PlayerCorpse corpse)
        {
            if (!init || corpse == null)
                return;
            
            if (corpse.playerSteamID.ToString().Contains("000") && !corpse.IsDestroyed)
                corpse.Kill();
        }

        void OnLootEntity(BasePlayer player, BasePlayer target)
        {
            if (init && target?.GetComponent<Fake>() != null && player && !player.IsAdmin)
                NextTick(() => player.EndLooting());
        }
        
        object CanNetworkTo(BasePlayer player, BasePlayer target)
        {
            if (player == null || target == null || player == target || target.IsAdmin)
                return null;

            if (awakers.Contains(player.net.ID))
                return false;

            return null;
        }

        void Unload()
        {
            var objects = GameObject.FindObjectsOfType(typeof(Fake));

            if (objects != null)
                foreach (var gameObj in objects)
                    UnityEngine.Object.Destroy(gameObj);

            shirts.Clear();
            pants.Clear();
            helms.Clear();
            vests.Clear();
            gloves.Clear();
            boots.Clear();
            weapons.Clear();
            awakers.Clear();
        }

        #region Config
        bool Changed;
        string szConsoleCommand;
        bool destroyFakeCorpses;
        bool preventFakeLooting;
        static bool stripFakes;
        static bool hideAwakers;
        float maxDistance;
        static float minDistance;
        static List<string> shirts = new List<string>();
        static List<string> pants = new List<string>();
        static List<string> helms = new List<string>();
        static List<string> vests = new List<string>();
        static List<string> gloves = new List<string>();
        static List<string> boots = new List<string>();
        static List<string> weapons = new List<string>();
        static string fakesName;

        void LoadVariables()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Failure"] = "Unable to find a position. Try looking at the ground or another object.",
            }, this);

            hideAwakers = Convert.ToBoolean(GetConfig("Invisibility", "Hide Real Awakers", false));
            stripFakes = Convert.ToBoolean(GetConfig("Settings", "Strip Fakes On Death", true));
            destroyFakeCorpses = Convert.ToBoolean(GetConfig("Settings", "Destroy Fake Corpses", true));
            preventFakeLooting = Convert.ToBoolean(GetConfig("Settings", "Prevent Fake Looting", true));
            szConsoleCommand = Convert.ToString(GetConfig("Settings", "Console Command", "createfake"));
            maxDistance = float.Parse(GetConfig("Settings", "Max Raycast Distance", 100f).ToString());
            minDistance = float.Parse(GetConfig("Settings", "Min Distance From Real Sleeper", 450f).ToString());
            fakesName = Convert.ToString(GetConfig("Settings", "Default Name If No Awakers", "luke"));

            var _shirts = GetConfig("Gear", "Shirts", DefaultShirts()) as List<object>;
            var _pants = GetConfig("Gear", "Pants", DefaultPants()) as List<object>;
            var _helms = GetConfig("Gear", "Helms", DefaultHelms()) as List<object>;
            var _vests = GetConfig("Gear", "Vests", DefaultVests()) as List<object>;
            var _gloves = GetConfig("Gear", "Gloves", DefaultGloves()) as List<object>;
            var _boots = GetConfig("Gear", "Boots", DefaultBoots()) as List<object>;
            var _weapons = GetConfig("Gear", "Weapons", DefaultWeapons()) as List<object>;

            if (_shirts.Count > 0)
                shirts.AddRange(_shirts.Select(shirt => shirt.ToString()));

            if (_pants.Count > 0)
                pants.AddRange(_pants.Select(pant => pant.ToString()));

            if (_helms.Count > 0)
                helms.AddRange(_helms.Select(helm => helm.ToString()));

            if (_vests.Count > 0)
                vests.AddRange(_vests.Select(vest => vest.ToString()));

            if (_gloves.Count > 0)
                gloves.AddRange(_gloves.Select(glove => glove.ToString()));

            if (_boots.Count > 0)
                boots.AddRange(_boots.Select(boot => boot.ToString()));

            if (_weapons.Count > 0)
                weapons.AddRange(_weapons.Select(weapon => weapon.ToString()));

            if (Changed)
            {
                SaveConfig();
                Changed = false;
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            Config.Clear();
            LoadVariables();
        }

        object GetConfig(string menu, string datavalue, object defaultValue)
        {
            var data = Config[menu] as Dictionary<string, object>;
            if (data == null)
            {
                data = new Dictionary<string, object>();
                Config[menu] = data;
                Changed = true;
            }
            object value;
            if (!data.TryGetValue(datavalue, out value))
            {
                value = defaultValue;
                data[datavalue] = value;
                Changed = true;
            }
            return value;
        }

        string msg(string key, string id = null, params object[] args) => string.Format(id == null ? RemoveFormatting(lang.GetMessage(key, this, id)) : lang.GetMessage(key, this, id), args);
        string RemoveFormatting(string source) => source.Contains(">") ? System.Text.RegularExpressions.Regex.Replace(source, "<.*?>", string.Empty) : source;
        #endregion
    }
}
