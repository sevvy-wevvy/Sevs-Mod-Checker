using UnityEngine;
using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using TMPro;
using Photon.Pun;
using GorillaNetworking;
using System.Collections;
using Valve.VR;
using UnityEngine.InputSystem;

namespace SevsModChecker
{
    [BepInPlugin("com.sev.gorillatag.SevsModChecker", "Sevs Mod Checker", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            AssetLoader.LoadAssets();
            GorillaTagger.OnPlayerSpawned(() => CustomStart());
        }

        internal GameObject go = null;
        internal bool HasStarted = false;
        List<GameObject> playerButtons = new List<GameObject>();
        List<TextMeshPro> playerButtonsText = new List<TextMeshPro>();
        internal GameObject SideButtons = null;

        internal string SelectedPlayerUserId = null;

        private void CustomStart()
        {
            GameObject prefab;
            if (AssetLoader.TryGetAsset<GameObject>("menu", out prefab))
            {
                go = Instantiate(prefab);
                go.name = "Sevs Mod Checker";
                /*if (false)
                {
                    go.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
                    go.transform.localPosition = new Vector3(0.16f, 0f, 0f);
                    go.transform.localRotation = Quaternion.Euler(20f, 180f, 10f);
                }
                else
                {
                    go.transform.LookAt(GorillaTagger.Instance.headCollider.transform);
                    go.transform.rotation = Quaternion.Euler(90f, go.transform.rotation.eulerAngles.y + 180, 0f);
                    go.transform.position = GorillaTagger.Instance.headCollider.transform.position + (GorillaTagger.Instance.headCollider.transform.forward * 0.5f) + new Vector3(0f, -0.2f, 0f);
                }*/
                go.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                go.SetActive(false);
            }

            int indexone = 0;
            foreach (Transform child in go.transform.Find("Player Buttons"))
            {
                int capturedIndex = indexone;
                child.AddComponent<Button>().Click += (b) => PlayerbUttonClick(capturedIndex);
                playerButtons.Add(child.gameObject);
                child.Find("text").TryGetComponent<TextMeshPro>(out TextMeshPro tmp);
                playerButtonsText.Add(tmp);
                child.gameObject.SetActive(false);
                indexone++;
            }

            SideButtons = go.transform.Find("Side Buttons").gameObject;
            foreach (Transform child in SideButtons.transform)
            {
                switch (child.name)
                {
                    case "Cosmetics":
                        child.AddComponent<Button>().Click += (b) => OpenCosmetics();
                        break;
                    case "Mods":
                        child.AddComponent<Button>().Click += (b) => OpenMods();
                        break;
                    case "Info":
                        child.AddComponent<Button>().Click += (b) => OpenInfo();
                        break;
                    case "Money":
                        child.AddComponent<Button>().Click += (b) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "https://sevvy-wevvy.com/donate/", UseShellExecute = true });
                        break;
                }
            }

            go.transform.Find("Players").AddComponent<Button>().Click += (b) => Home();

            ModText = go.transform.Find("Mod Text").GetComponent<TextMeshPro>();
            MainName = go.transform.Find("Name Main").GetComponent<TextMeshPro>();
            CosmeticText = go.transform.Find("Cosmetic Text").GetComponent<TextMeshPro>();
            InfoMenu = go.transform.Find("Info Menu").gameObject;

            Name = InfoMenu.transform.Find("Name").GetComponent<TextMeshPro>();
            ModAndcosmeticCount = InfoMenu.transform.Find("mods and cosmetics").GetComponent<TextMeshPro>();
            Platform = InfoMenu.transform.Find("Platform").GetComponent<TextMeshPro>();
            Fps = InfoMenu.transform.Find("fps").GetComponent<TextMeshPro>();
            CreationDate = InfoMenu.transform.Find("creatoin date").GetComponent<TextMeshPro>();
            thing = InfoMenu.transform.Find("thing").gameObject;

            cheatPrecent = InfoMenu.transform.Find("cheat precent").GetComponent<TextMeshPro>();
            modsTiny = InfoMenu.transform.Find("mods tiny").GetComponent<TextMeshPro>();

            HasStarted = true;
        }

        internal TextMeshPro CosmeticText = null;
        internal TextMeshPro MainName = null;
        internal TextMeshPro ModText = null;
        internal GameObject InfoMenu = null;

        internal TextMeshPro Name = null;
        internal TextMeshPro ModAndcosmeticCount = null;
        internal TextMeshPro Platform = null;
        internal TextMeshPro Fps = null;
        internal TextMeshPro CreationDate = null;
        internal TextMeshPro cheatPrecent = null;
        internal TextMeshPro modsTiny = null;
        internal GameObject thing = null;

        internal Photon.Realtime.Player[] PlayerlIstGameNetwroking = null;
        internal bool MenuToogleDone = false;
        internal bool InfoOpen = false;

        static FieldInfo moddedField =
        typeof(VRRig).GetField("wasInModdedRoom",
        BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool HasBeenModdedThisSession(VRRig rig)
        {
            return rig != null

                && moddedField != null
                && (bool)moddedField.GetValue(rig);
        }

        bool setParent = true;
        Transform parent = null;
        Camera thirdPersonCamera = null;

        bool onPC = false;

        KeyCode openKey = KeyCode.F2;

        internal void Update()
        {
            if (thirdPersonCamera == null)
            {
                try
                {
                    thirdPersonCamera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                }
                catch
                {
                    thirdPersonCamera = GameObject.Find("Shoulder Camera").GetComponent<Camera>();
                }
            }

            PlayerlIstGameNetwroking = PhotonNetwork.PlayerListOthers;

            if ((SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand) || UnityInput.Current.GetKeyDown(openKey)) && !MenuToogleDone)
            {
                if (UnityInput.Current.GetKeyDown(openKey)) onPC = true;
                else if (SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand)) onPC = false;

                go.SetActive(!go.activeSelf);
                MenuToogleDone = true;
            }
            if (!(SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand) || UnityInput.Current.GetKeyDown(openKey))) MenuToogleDone = false;
            if (go.activeSelf)
            {
                if (parent == null && setParent)
                {
                    parent = go.transform.parent;
                    setParent = false;
                }
                if (onPC)
                {
                    go.transform.parent = thirdPersonCamera.transform;
                    go.transform.position = thirdPersonCamera.transform.position + thirdPersonCamera.transform.forward * 0.5f;
                    go.transform.rotation = thirdPersonCamera.transform.rotation * Quaternion.Euler(90f, 180f, 0f);

                    if (Mouse.current.leftButton.isPressed)
                    {
                        if (Physics.Raycast(thirdPersonCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hitInfo, 512, ~(1 << LayerMask.NameToLayer("TransparentFX") | 1 << LayerMask.NameToLayer("Ignore Raycast") | 1 << LayerMask.NameToLayer("Zone") | 1 << LayerMask.NameToLayer("Gorilla Trigger") | 1 << LayerMask.NameToLayer("Gorilla Boundary") | 1 << LayerMask.NameToLayer("GorillaCosmetics") | 1 << LayerMask.NameToLayer("GorillaParticle"))))
                        {
                            Button button = hitInfo.transform.gameObject.GetComponent<Button>();

                            if (button != null) button.CustomClick();
                        }
                    }
                }
                else
                {
                    go.transform.parent = parent;
                    go.transform.position = GorillaTagger.Instance.leftHandTransform.position + GorillaTagger.Instance.leftHandTransform.right * 0.16f;
                    go.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation * Quaternion.Euler(20f, 180f, 10f);
                }
            }

            if (!HasStarted) return;

            Photon.Realtime.Player selectedPlayer = null;
            if (!string.IsNullOrEmpty(SelectedPlayerUserId))
                selectedPlayer = PlayerlIstGameNetwroking.FirstOrDefault(p => p.UserId == SelectedPlayerUserId);

            if (selectedPlayer != null)
            {
                MainName.text = selectedPlayer.NickName;
                calctulacteCheater(selectedPlayer);
            }
            else
            {
                SelectedPlayerUserId = null;
                MainName.text = "Select a Player";
            }

            for (int i = 0; i < playerButtons.Count; i++)
            {
                if (i < PlayerlIstGameNetwroking.Length && i < playerButtonsText.Count)
                {
                    playerButtons[i].SetActive(true);
                    playerButtonsText[i].text = PlayerlIstGameNetwroking[i].NickName + " || " + calctulacteCheaterPlayer(PlayerlIstGameNetwroking[i]);
                    if (!string.IsNullOrEmpty(GetModsSmall(PlayerlIstGameNetwroking[i]))) playerButtonsText[i].text += "\n<size=3>" + GetModsSmall(PlayerlIstGameNetwroking[i]) + "</size>";
                }
                else
                {
                    playerButtons[i].SetActive(false);
                }
            }

            SideButtons.SetActive(!string.IsNullOrEmpty(SelectedPlayerUserId));

            if (InfoOpen && selectedPlayer != null)
                runInfoOpen(selectedPlayer);
        }

        internal void PlayerbUttonClick(int index)
        {
            if (index < PlayerlIstGameNetwroking.Length)
                SelectedPlayerUserId = PlayerlIstGameNetwroking[index].UserId;
        }

        public void calctulacteCheater(Photon.Realtime.Player ply)
        {
            int CheatScale = 0;
            int modcount = CheckMods(RigManager.GetVRRigFromPlayer(ply)).Count;
            if (modcount > 0) CheatScale += 10;

            if ((RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "PC") || (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "STEAM")) CheatScale += 20;

            if (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "UNKNOWN") CheatScale += 10;

            int fps = int.Parse(RigManager.GetFPS(RigManager.GetVRRigFromPlayer(ply)));
            if (fps < 60) CheatScale += 10;
            if (fps < 30) CheatScale += 30;

            if (HasBeenModdedThisSession(RigManager.GetVRRigFromPlayer(ply))) CheatScale += 20;

            if (CheatScale > 100) CheatScale = 100;

            cheatPrecent.text = $"Cheat Percent: <color={(CheatScale >= 70 ? "red" : (CheatScale >= 40 ? "yellow" : "green"))}>{CheatScale}%</color>";
            thing.transform.localPosition = Vector3.Lerp(thing.transform.localPosition, new Vector3(2f - ((CheatScale / 100f) * 4f), thing.transform.localPosition.y, thing.transform.localPosition.z), Time.deltaTime * 8f);
        }

        public string calctulacteCheaterPlayer(Photon.Realtime.Player ply)
        {
            int CheatScale = 0;
            int modcount = CheckMods(RigManager.GetVRRigFromPlayer(ply)).Count;
            if (modcount > 0) CheatScale += 10;

            if ((RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "PC") || (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "STEAM")) CheatScale += 20;

            if (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "UNKNOWN") CheatScale += 10;

            int fps = int.Parse(RigManager.GetFPS(RigManager.GetVRRigFromPlayer(ply)));
            if (fps < 60) CheatScale += 10;
            if (fps < 30) CheatScale += 30;

            if (HasBeenModdedThisSession(RigManager.GetVRRigFromPlayer(ply))) CheatScale += 20;

            if (CheatScale > 100) CheatScale = 100;

            return $"<color={(CheatScale >= 70 ? "red" : (CheatScale >= 40 ? "yellow" : "green"))}>{CheatScale}%</color>";
        }

        public string GetModsSmall(Photon.Realtime.Player ply)
        {
            List<string> mods = new List<string>();
            if (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "STEAM") mods.Add("Steam");
            if (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "PC") mods.Add("PC");
            if (RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply)) == "UNKNOWN") mods.Add("Unknown Platform");
            int fps = int.Parse(RigManager.GetFPS(RigManager.GetVRRigFromPlayer(ply)));
            if (fps < 60) mods.Add("Low FPS/HZ");
            if (fps < 30) mods.Add("Extreamly low FPS/HZ");
            int modcount = CheckMods(RigManager.GetVRRigFromPlayer(ply)).Count;
            if (modcount > 0) mods.Add("Mods");
            if (HasBeenModdedThisSession(RigManager.GetVRRigFromPlayer(ply))) mods.Add("In Modded");

            return mods.Join(", ");
        }

        internal void Home()
        {
            otherfalse();
            go.transform.Find("Player Buttons").gameObject.SetActive(true);
        }

        internal void otherfalse()
        {
            ModText.gameObject.SetActive(false);
            CosmeticText.gameObject.SetActive(false);
            go.transform.Find("Player Buttons").gameObject.SetActive(false);
            InfoMenu.gameObject.SetActive(false);
            InfoOpen = false;
        }

        internal void OpenCosmetics()
        {
            var ply = PlayerlIstGameNetwroking.FirstOrDefault(p => p.UserId == SelectedPlayerUserId);
            if (ply == null) return;

            var cosmetics = CheckCosmetics(RigManager.GetVRRigFromPlayer(ply));
            CosmeticText.text = cosmetics.Any() ? cosmetics.Join(", ") : "No Special Cosmetics Found";
            otherfalse();
            CosmeticText.gameObject.SetActive(true);
        }

        internal void OpenMods()
        {
            var ply = PlayerlIstGameNetwroking.FirstOrDefault(p => p.UserId == SelectedPlayerUserId);
            if (ply == null) return;

            var mods = CheckMods(RigManager.GetVRRigFromPlayer(ply));
            ModText.text = mods.Any() ? mods.Join(", ") : "No Mods Found";
            otherfalse();
            ModText.gameObject.SetActive(true);
        }

        internal void OpenInfo()
        {
            otherfalse();
            InfoOpen = true;
            InfoMenu.gameObject.SetActive(true);
        }

        internal void runInfoOpen(Photon.Realtime.Player ply)
        {
            Name.text = ply.NickName;
            ModAndcosmeticCount.text = $"<color=green>{CheckMods(RigManager.GetVRRigFromPlayer(ply)).Count}</color> mods & <color=green>{CheckCosmetics(RigManager.GetVRRigFromPlayer(ply)).Count}</color> special cosmetics";
            Platform.text = "Platform: " + RigManager.GetPlatform(RigManager.GetVRRigFromPlayer(ply));
            Fps.text = "FPS: " + RigManager.GetFPS(RigManager.GetVRRigFromPlayer(ply));
            CreationDate.text = "Creation Date: " + RigManager.CreationDate(RigManager.GetVRRigFromPlayer(ply));
            modsTiny.text = GetModsSmall(ply);
        }

        public List<string> CheckCosmetics(VRRig rig)
        {
            List<string> specialties = new List<string>();

            Dictionary<string, string[]> specialCosmetics = new Dictionary<string, string[]> {
                { "LBAAD.", new string[] { "ADMINISTRATOR", "FF0000" } },
                { "LBAAK.", new string[] { "FOREST GUIDE", "867556" } },
                { "LBADE.", new string[] { "FINGER PAINTER", "00FF00" } },
                { "LBAGS.", new string[] { "ILLUSTRATOR", "C76417" } },
                { "LMAPY.", new string[] { "FOREST GUIDE MOD STICK", "FF8000" } },
                { "LBANI.", new string[] { "AA CREATOR BADGE", "291447" } } };

            foreach (KeyValuePair<string, string[]> specialCosmetic in specialCosmetics)
            {
                if (rig.concatStringOfCosmeticsAllowed.Contains(specialCosmetic.Key))
                    specialties.Add("<color=#" + specialCosmetic.Value[1] + ">" + specialCosmetic.Value[0] + "</color>");
            }

            return specialties.IsNullOrEmpty() ? new List<string>() : specialties;
        }

        public Dictionary<string, string[]> specialModsList = new Dictionary<string, string[]> {
        { "genesis", new string[] { "GENESIS", "07019C" } },
        { "HP_Left", new string[] { "HOLDABLEPAD", "332316" } },
        { "GrateVersion", new string[] { "GRATE", "707070" } },
        { "void", new string[] { "VOID", "FFFFFF" } },
        { "BANANAOS", new string[] { "BANANAOS", "FFFF00" } },
        { "GC", new string[] { "GORILLACRAFT", "43B581" } },
        { "CarName", new string[] { "GORILLAVEHICLES", "43B581" } },
        { "6p72ly3j85pau2g9mda6ib8px", new string[] { "CCMV2", "BF00FC" } },
        { "FPS-Nametags for Zlothy", new string[] { "FPSTAGS", "B103FC" } },
        { "cronos", new string[] { "CRONOS", "0000FF" } },
        { "ORBIT", new string[] { "ORBIT", "FFFFFF" } },
        { "Violet On Top", new string[] { "VIOLET", "DF6BFF" } },
        { "MP25", new string[] { "MONKEPHONE", "707070" } },
        { "GorillaWatch", new string[] { "GORILLAWATCH", "707070" } },
        { "InfoWatch", new string[] { "GORILLAINFOWATCH", "707070" } },
        { "BananaPhone", new string[] { "BANANAPHONE", "FFFC45" } },
        { "Vivid", new string[] { "VIVID", "F000BC" } },
        { "RGBA", new string[] { "CUSTOMCOSMETICS", "FF0000" } },
        { "cheese is gouda", new string[] { "WHOSICHEATING", "707070" } },
        { "shirtversion", new string[] { "GORILLASHIRTS", "707070" } },
        { "gpronouns", new string[] { "GORILLAPRONOUNS", "707070" } },
        { "gfaces", new string[] { "GORILLAFACES", "707070" } },
        { "monkephone", new string[] { "MONKEPHONE", "707070" } },
        { "pmversion", new string[] { "PLAYERMODELS", "707070" } },
        { "gtrials", new string[] { "GORILLATRIALS", "707070" } },
        { "msp", new string[] { "MONKESMARTPHONE", "707070" } },
        { "gorillastats", new string[] { "GORILLASTATS", "707070" } },
        { "using gorilladrift", new string[] { "GORILLADRIFT", "707070" } },
        { "monkehavocversion", new string[] { "MONKEHAVOC", "707070" } },
        { "tictactoe", new string[] { "TICTACTOE", "a89232" } },
        { "ccolor", new string[] { "INDEX", "0febff" } },
        { "imposter", new string[] { "GORILLAAMONGUS", "ff0000" } },
        { "spectapeversion", new string[] { "SPECTAPE", "707070" } },
        { "cats", new string[] { "CATS", "707070" } },
        { "made by biotest05 :3", new string[] { "DOGS", "707070" } },
        { "fys cool magic mod", new string[] { "FYSMAGICMOD", "707070" } },
        { "colour", new string[] { "CUSTOMCOSMETICS", "707070" } },
        { "chainedtogether", new string[] { "CHAINED TOGETHER", "707070" } },
        { "goofywalkversion", new string[] { "GOOFYWALK", "707070" } },
        { "void_menu_open", new string[] { "VOID", "303030" } },
        { "violetpaiduser", new string[] { "VIOLETPAID", "DF6BFF" } },
        { "violetfree", new string[] { "VIOLETFREE", "DF6BFF" } },
        { "obsidianmc", new string[] { "OBSIDIAN.LOL", "303030" } },
        { "dark", new string[] { "SHIBAGT DARK", "303030" } },
        { "hidden menu", new string[] { "HIDDEN", "707070" } },
        { "oblivionuser", new string[] { "OBLIVION", "5055d3" } },
        { "hgrehngio889584739_hugb\n", new string[] { "RESURGENCE", "470050" } },
        { "eyerock reborn", new string[] { "EYEROCK", "707070" } },
        { "asteroidlite", new string[] { "ASTEROID LITE", "707070" } },
        { "elux", new string[] { "ELUX", "707070" } },
        { "cokecosmetics", new string[] { "COKE COSMETX", "00ff00" } },
        { "GFaces", new string[] { "gFACES", "707070" } },
        { "github.com/maroon-shadow/SimpleBoards", new string[] { "SIMPLEBOARDS", "707070" } },
        { "ObsidianMC", new string[] { "OBSIDIAN", "DC143C" } },
        { "hgrehngio889584739_hugb", new string[] { "RESURGENCE", "707070" } },
        { "GTrials", new string[] { "gTRIALS", "707070" } },
        { "github.com/ZlothY29IQ/GorillaMediaDisplay", new string[] { "GMD", "B103FC" } },
        { "github.com/ZlothY29IQ/TooMuchInfo", new string[] { "TOOMUCHINFO", "B103FC" } },
        { "github.com/ZlothY29IQ/RoomUtils-IW", new string[] { "ROOMUTILS-IW", "B103FC" } },
        { "github.com/ZlothY29IQ/MonkeClick", new string[] { "MONKECLICK", "B103FC" } },
        { "github.com/ZlothY29IQ/MonkeClick-CI", new string[] { "MONKECLICK-CI", "B103FC" } },
        { "github.com/ZlothY29IQ/MonkeRealism", new string[] { "MONKEREALISM", "B103FC" } },
        { "MediaPad", new string[] { "MEDIAPAD", "B103FC" } },
        { "GorillaCinema", new string[] { "gCINEMA", "B103FC" } },
        { "ChainedTogetherActive", new string[] { "CHAINEDTOGETHER", "B103FC" } },
        { "GPronouns", new string[] { "gPRONOUNS", "707070" } },
        { "CSVersion", new string[] { "CustomSkin", "707070" } },
        { "github.com/ZlothY29IQ/Zloth-RecRoomRig", new string[] { "ZLOTH-RRR", "B103FC" } },
        { "ShirtProperties", new string[] { "SHIRTS-OLD", "707070" } },
        { "GorillaShirts", new string[] { "SHIRTS", "707070" } },
        { "GS", new string[] { "OLD SHIRTS", "707070" } },
        { "6XpyykmrCthKhFeUfkYGxv7xnXpoe2", new string[] { "CCMV2", "DC143C" } },
        { "Body Tracking", new string[] { "BODYTRACK-OLD", "7AA11F" } },
        { "Body Estimation", new string[] { "HANBodyEst", "7AA11F" } },
        { "Gorilla Track", new string[] { "BODYTRACK", "7AA11F" } },
        { "CustomMaterial", new string[] { "CUSTOMCOSMETICS", "707070" } },
        { "I like cheese", new string[] { "RECROOMRIG", "FE8232" } },
        { "silliness", new string[] { "SILLINESS", "FFBAFF" } },
        { "emotewheel", new string[] { "EMOTEWHEEL", "1E2030" } },
        { "untitled", new string[] { "UNTITLED", "2D73AF" } }
    };

        public List<string> CheckMods(VRRig rig)
        {
            List<string> specialMods = new List<string>();
            NetPlayer creator = rig.Creator;

            Dictionary<string, object> customProps = new Dictionary<string, object>();
            foreach (DictionaryEntry dictionaryEntry in creator.GetPlayerRef().CustomProperties)
                customProps[dictionaryEntry.Key.ToString().ToLower()] = dictionaryEntry.Value;

            bool hasAllMods = true;
            foreach (KeyValuePair<string, string[]> specialMod in specialModsList)
            {
                if (customProps.ContainsKey(specialMod.Key.ToLower()))
                    specialMods.Add("<color=#" + specialMod.Value[1].ToUpper() + ">" + specialMod.Value[0].ToUpper() + "</color>");
                else
                    hasAllMods = false;
            }

            if (hasAllMods)
                return new List<string> { "<color=red>Using Mod Spoofer</color>" };

            CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
            foreach (CosmeticsController.CosmeticItem cosmetic in cosmeticSet.items)
            {
                if (!cosmetic.isNullItem && !rig.concatStringOfCosmeticsAllowed.Contains(cosmetic.itemName))
                {
                    specialMods.Add("<color=green>COSMETX</color>");
                    break;
                }
            }

            return specialMods.IsNullOrEmpty() ? new List<string>() : specialMods;
        }
    }

    internal static class AssetLoader
    {
        private static readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        public static bool BundleLoaded => _assets.Count > 0;

        public static bool TryGetAsset<T>(string name, out T obj) where T : UnityEngine.Object
        {
            if (BundleLoaded && _assets.FirstOrDefault(asset => asset.name == name) is T prefab)
            {
                obj = prefab;
                return true;
            }

            obj = null!;
            return false;
        }

        public static void LoadAssets()
        {
            try
            {
                if (BundleLoaded) throw new Exception("Assets already loaded.");
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream? stream = assembly.GetManifestResourceStream($"SevsModChecker.smc");
                AssetBundle bundle = AssetBundle.LoadFromStream(stream ?? throw new Exception("Failed to get stream."));

                UnityEngine.Debug.Log($"Retrieved bundle: {(bundle ?? throw new Exception("Failed to get bundle.")).name}");
                foreach (var asset in bundle.LoadAllAssets())
                {
                    _assets.AddIfNew(asset);
                    UnityEngine.Debug.Log($"Loaded asset: {asset.name} ({asset.GetType().FullName})");
                }

                stream.Close();
                UnityEngine.Debug.Log($"Loaded {_assets.Count} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }
    }
}