using GTA; // This is a reference that is needed! do not edit this
using GTA.Native; // This is a reference that is needed! do not edit this
using System; // This is a reference that is needed! do not edit this
using System.Windows.Forms; // This is a reference that is needed! do not edit this
using GTA.Math;
using SimpleUI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using XMLSerializationHelper;
using ParticleFxHelper;
using FxXmlLayout;
using ScriptCommunicatorHelper;
using System.Globalization;
using Control = GTA.Control;
using PtfxMemoryAccess = Memory.PtfxMemoryAccess;

namespace ParticleFxPlayer
{
    public class MainScript : Script // declare Modname as a script
    {
        bool SCHelperExists;
        ScriptCommunicator PtfxPlayerComm = new ScriptCommunicator("ParticleFxPlayer");

        int fxHandle;
        float fxSize = 4.0f;
        float colourR = 255f;
        float colourG = 0f;
        float colourB = 255f;
        float rangeFx = 0f;

        string lastAsset;
        FxName lastFXName;

        bool PlayerIsTaskedToMove;
        bool allowMenuDraw = true;

        Keys MenuKey;
        Keys MenuKey2;
        Control buttonToggle1;
        Control buttonToggle2;
        Control buttonToggle3;

        string searchString;
        bool doSearch;

        int favLastIndex;
        bool rebuildFavourites;
        
        List<FxAsset> FxAssets = new List<FxAsset>();
        List<DetailedFx> FavouritesList = new List<DetailedFx>();

        MenuPool _menuPool;
        UIMenu FXMenu;
        UIMenu FxListMenu;
        UIMenu FavouritesMenu;
        UIMenuItem ItemFxSize;
        UIMenuItem ItemColourR;
        UIMenuItem ItemColourG;
        UIMenuItem ItemColourB;
        UIMenuItem ItemRange;
        UIMenuItem ItemRemoveNearbyFx;
        UIMenuItem ItemMoveTask;

        UIMenuItem ItemSearch;
        UIMenu SearchMenu;

        CultureInfo culture;

        DateTime InputTimer;

        public MainScript() // main function
        {
            PtfxMemoryAccess.Init();
            SetupKeyboardCulture();
            InitSCHelper();
            CreateFavouritesXML(); //only if missing
            CreateIni(); //only if missing
            ReadINI();
            ReadSCHelperINI();

            FxAssets = XMLHelper.LoadXMLToObject(FxAssets, @"scripts\ParticleFxList.xml");
            FavouritesList = XMLHelper.LoadXMLToObject(FavouritesList, @"scripts\ParticleFxFavourites.xml");

            InitMenu();
            
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Aborted += OnAbort;

            //Interval = 10;
        }

        void SetupKeyboardCulture()
        {
            culture = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name, true);
            culture.NumberFormat.NumberDecimalSeparator = ".";
            ForceDecimal();
        }

        void ForceDecimal()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }

        private void OnAbort(object sender, EventArgs e)
        {
            PTFXHelper.RemovePTFX(fxHandle);
            ForceRemoveSurroundingFX(Game.Player.Character);
        }

        void InitSCHelper()
        {
            SCHelperExists = PtfxPlayerComm.ScriptCommunicatorMenuDllExists();

            CreateSCMOD();
        }

        void InitMenu()
        {
            _menuPool = new MenuPool();

            FXMenu = new UIMenu("Simple FX Player");
            _menuPool.AddMenu(FXMenu);
            FXMenu.TitleColor = System.Drawing.Color.FromArgb(255, 237, 90, 90);
            FXMenu.TitleBackgroundColor = System.Drawing.Color.FromArgb(240, 0, 0, 0);
            FXMenu.TitleUnderlineColor = System.Drawing.Color.FromArgb(255, 237, 90, 90);
            FXMenu.DefaultBoxColor = System.Drawing.Color.FromArgb(160, 0, 0, 0);
            FXMenu.DefaultTextColor = System.Drawing.Color.FromArgb(230, 255, 255, 255);
            FXMenu.HighlightedBoxColor = System.Drawing.Color.FromArgb(130, 237, 90, 90);
            FXMenu.HighlightedItemTextColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            FXMenu.DescriptionBoxColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);
            FXMenu.DescriptionTextColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);

            FxListMenu = new UIMenu("Particle Fx List");
            _menuPool.AddSubMenu(FxListMenu, FXMenu, FxListMenu.Title);

            foreach (var dic in FxAssets)
            {
                UIMenu assetMenu = new UIMenu(dic.AssetName);
                _menuPool.AddSubMenu(assetMenu, FxListMenu, dic.AssetName);

                dic.FxNames = dic.FxNames.Distinct().ToList();

                foreach (var fx in dic.FxNames)
                {
                    int evoListCount = fx.EvolutionList.Count;
                    if (evoListCount > 1)
                    {
                        UIMenu fxNameMenu = new UIMenu(fx.PTFXName);
                        _menuPool.AddSubMenu(fxNameMenu, assetMenu, fx.PTFXName, "May Require Evolution Arguments.");

                        foreach (var e in fx.EvolutionList)
                        {
                            UIMenuItem evoName = new UIMenuItem("Evolution Name: " + e.EvolutionName, "< " + e.Amount + " >", "Hold SHIFT and select to save to favourites.");
                            fxNameMenu.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, evoName, fx, dic.AssetName, true);
                            fxNameMenu.OnItemLeftRight += (s, selItem, selIndex, direction) => FxEvolution_OnItemLeftRight(s, selItem, selIndex, direction, evoName, fx.PTFXName, dic.AssetName, true, e, e.EvolutionName);
                            fxNameMenu.AddMenuItem(evoName);
                        }
                    }
                    else
                    {
                        UIMenuItem fxnameItem = new UIMenuItem(fx.PTFXName);
                        if (evoListCount == 1)
                        {
                            fxnameItem.Value = "< " + fx.EvolutionList[0].Amount + " >";
                            fxnameItem.Description = "Has one Evolution argument: " + fx.EvolutionList[0].EvolutionName + ". Hold SHIFT and select to save to favourites.";
                            assetMenu.OnItemLeftRight += (s, selItem, selIndex, direction) => FxEvolution_OnItemLeftRight(s, selItem, selIndex, direction, fxnameItem, fx.PTFXName, dic.AssetName, true, fx.EvolutionList[0], fx.EvolutionList[0].EvolutionName);
                            assetMenu.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, fxnameItem, fx, dic.AssetName, true);
                        }
                        else
                        {
                            fxnameItem.Description = "No Evolution arguments found. Hold SHIFT and select to save to favourites.";
                            assetMenu.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, fxnameItem, fx, dic.AssetName);
                        }

                        assetMenu.AddMenuItem(fxnameItem);
                    }
                }

                UIMenuItem dumpAssetInfoItem = new UIMenuItem("Dump PTFX info", null, "Dumps info from memory in case there are missing Evolution parameters. Requires script restart to see changes, if any.");
                assetMenu.OnItemSelect += (s, selItem, selIndex) => AssetDump_OnItemSelect(s, selItem, selIndex, dumpAssetInfoItem, dic.AssetName);
                assetMenu.AddMenuItem(dumpAssetInfoItem);
            }

            FavouritesMenu = new UIMenu("Favourites");
            _menuPool.AddSubMenu(FavouritesMenu, FXMenu, "Favourites");

            Re_BuildFavouritesMenu();

            SearchMenu = new UIMenu("Search Results");
            SearchMenu.ParentMenu = FXMenu;
            _menuPool.AddMenu(SearchMenu);
            ItemSearch = new UIMenuItem("Search", null, "Search fx names that match whatever you type. Ex: bang, water, veh.");
            FXMenu.AddMenuItem(ItemSearch);
            SearchMenu.TitleColor = System.Drawing.Color.FromArgb(255, 237, 90, 90);
            SearchMenu.TitleBackgroundColor = System.Drawing.Color.FromArgb(240, 0, 0, 0);
            SearchMenu.TitleUnderlineColor = System.Drawing.Color.FromArgb(255, 237, 90, 90);
            SearchMenu.DefaultBoxColor = System.Drawing.Color.FromArgb(160, 0, 0, 0);
            SearchMenu.DefaultTextColor = System.Drawing.Color.FromArgb(230, 255, 255, 255);
            SearchMenu.HighlightedBoxColor = System.Drawing.Color.FromArgb(130, 237, 90, 90);
            SearchMenu.HighlightedItemTextColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
            SearchMenu.DescriptionBoxColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);
            SearchMenu.DescriptionTextColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);

            ItemFxSize = new UIMenuItem("FX Size");
            ItemFxSize.Value = "< " + fxSize + " >";
            FXMenu.AddMenuItem(ItemFxSize);

            ItemColourR = new UIMenuItem("FX Colour [Red]");
            ItemColourR.Value = "< " + colourR + " >";
            ItemColourR.Description = "Move left/right to change value. Select to play last played FX. Only works for some fx.";
            FXMenu.AddMenuItem(ItemColourR);

            ItemColourG = new UIMenuItem("FX Colour [Green]");
            ItemColourG.Value = "< " + colourG + " >";
            ItemColourG.Description = "Move left/right to change value. Select to play last played FX. Only works for some fx.";
            FXMenu.AddMenuItem(ItemColourG);

            ItemColourB = new UIMenuItem("FX Colour [Blue]");
            ItemColourB.Value = "< " + colourB + " >";
            ItemColourB.Description = "Move left/right to change value. Select to play last played FX. Only works for some fx.";
            FXMenu.AddMenuItem(ItemColourB);

            ItemRange = new UIMenuItem("FX Range");
            ItemRange.Value = "< " + rangeFx + " >";
            ItemRange.Description = "Move left/right to change value. Select to play last played FX. May only work for certain fx.";
            FXMenu.AddMenuItem(ItemRange);

            ItemRemoveNearbyFx = new UIMenuItem("Remove last and nearby fx", null, "Use if there is an effect that isn't going away");
            FXMenu.AddMenuItem(ItemRemoveNearbyFx);

            ItemMoveTask = new UIMenuItem("Set Player To Walk/Drive Automatically", null, "Some ptfx are harder to see when stationary");
            FXMenu.AddMenuItem(ItemMoveTask);

            FXMenu.OnItemSelect += FXMenu_OnItemSelect;
            FXMenu.OnItemLeftRight += FXMenu_OnItemLeftRight;
        }

        private void FXMenu_OnItemLeftRight(UIMenu sender, UIMenuItem selectedItem, int index, bool left)
        {
            if (selectedItem == ItemFxSize)
            {
                fxSize = FXMenu.ControlFloatValue(ItemFxSize, left, fxSize, 0.1f, 1.0f);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetScale(fxHandle, fxSize);
                }
            }

            else if (selectedItem == ItemColourR)
            {
                colourR = FXMenu.ControlFloatValue(ItemColourR, left, colourR, 1f, 10f, 0, true, 0f, 255.0f);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetColour(fxHandle, colourR, colourG, colourB);
                }
            }

            else if ( selectedItem == ItemColourG)
            {
                colourG = FXMenu.ControlFloatValue(ItemColourG, left, colourG, 1f, 10f, 0, true, 0f, 255.0f);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetColour(fxHandle, colourR, colourG, colourB);
                }
            }

            else if (selectedItem == ItemColourB)
            {
                colourB = FXMenu.ControlFloatValue(ItemColourB, left, colourB, 1f, 10f, 0, true, 0f, 255.0f);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetColour(fxHandle, colourR, colourG, colourB);
                }
            }

            else if (selectedItem == ItemRange)
            {
                rangeFx = FXMenu.ControlFloatValue(ItemRange, left, rangeFx, 0.1f, 1f, 1);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetRange(fxHandle, rangeFx);
                }
            }
        }

        private void FXMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == ItemRemoveNearbyFx)
            {
                ForceRemoveSurroundingFX(Game.Player.Character);
            }

            if (selectedItem == ItemSearch)
            {
                searchString = Game.GetUserInput(25);
                if (searchString == null || searchString == "") { FXMenu.IsVisible = false; SearchMenu.IsVisible = true; return; }
                SearchMenu.ResetIndexPosition();
                SearchMenu.SelectedItem = null;
                if (SearchMenu.UIMenuItemList.Count > 0) { SearchMenu.UIMenuItemList.Clear(); }
                FXMenu.IsVisible = false; SearchMenu.IsVisible = true;

                doSearch = true;
                allowMenuDraw = false;
            }
            
            if (selectedItem == ItemMoveTask)
            {
                if (PlayerIsTaskedToMove)
                {
                    Game.Player.Character.Task.ClearAll();
                    PlayerIsTaskedToMove = false;
                }
                else
                {
                    if (Game.Player.Character.IsInVehicle())
                    {
                        Game.Player.Character.Task.CruiseWithVehicle(Game.Player.Character.CurrentVehicle, 50f, 6); //gtaforums.com/topic/822314-guide-driving-styles/
                    }
                    else
                    {
                        Game.Player.Character.Task.WanderAround();
                    }
                    PlayerIsTaskedToMove = true;
                }
            }

            if (selectedItem == ItemColourR || selectedItem == ItemColourG || selectedItem == ItemColourB || selectedItem == ItemFxSize || selectedItem == ItemRange)
            {
                if (lastAsset != null && lastFXName != null)
                {
                    PTFXHelper.RemovePTFX(fxHandle);

                    Ped player = Game.Player.Character;

                    PlayLoopOrNonLoopedFX(player.IsInVehicle() ? player.CurrentVehicle : (Entity)player, lastAsset, lastFXName, lastFXName.EvolutionList.Count > 0 ? true : false);

                    if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                    {
                        if (selectedItem == ItemColourR || selectedItem == ItemColourG || selectedItem == ItemColourB)
                        {
                            PTFXHelper.SetColour(fxHandle, colourR, colourG, colourB);
                        }
                        else if (selectedItem == ItemRange)
                        {
                            PTFXHelper.SetColour(fxHandle, colourR, colourG, colourB);
                            PTFXHelper.SetRange(fxHandle, rangeFx);
                        }
                    }
                }
            }
        }

        private void FxHighlight_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index, UIMenuItem itemToControl, FxName fxName, string asset, bool useEvolutionArg = false)
        {
           if (selectedItem == itemToControl)
            {
                PTFXHelper.RemovePTFX(fxHandle);

                if (itemToControl.Text == "REMOVED") return;

                Ped player = Game.Player.Character;

                PlayLoopOrNonLoopedFX(player.IsInVehicle() ? player.CurrentVehicle : (Entity)player, asset, fxName, useEvolutionArg);

                lastAsset = asset;
                lastFXName = fxName;
                
                if (Game.IsKeyPressed(Keys.ShiftKey))
                {
                    if (sender == FavouritesMenu || (sender.ParentMenu != null && sender.ParentMenu == FavouritesMenu)) //Remove from favourites
                    {
                        try
                        {
                            FavouritesList.Remove(FavouritesList.Find(d => d.FxName == fxName && d.AssetName == asset));
                            ForceDecimal();
                            XMLHelper.SaveObjectToXML(FavouritesList, @"scripts\ParticleFxFavourites.xml");

                            if (sender == FavouritesMenu)
                            {
                                itemToControl.Text = "REMOVED";
                                itemToControl.Value = null;
                                itemToControl.Description = null;
                            }
                            else if (sender.ParentMenu != null && sender.ParentMenu == FavouritesMenu)
                            {
                                sender.Title = "REMOVED";
                                sender.ParentItem.Text = "REMOVED";
                                sender.ParentItem.Description = null;
                            }

                            UI.ShowSubtitle("Removed from Favourites!");
                        }
                        catch { }
                    }
                    else //Add to favourites
                    {
                        DetailedFx newFav = new DetailedFx(asset, fxName);
                        FavouritesList.Add(newFav);
                        ForceDecimal();
                        XMLHelper.SaveObjectToXML(FavouritesList, @"scripts\ParticleFxFavourites.xml");
                        UI.ShowSubtitle("Saved to Favourites!");

                        favLastIndex = FavouritesMenu.SelectedIndex;
                        rebuildFavourites = true;
                        allowMenuDraw = false;
                    }
                }
            }
        }

        private void AssetDump_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index, UIMenuItem itemToControl, string asset)
        {
            if (selectedItem == itemToControl)
            {
                PTFXDumpToXML.DumpToCamxxCoreStyleTxt(asset);
                UI.ShowSubtitle("Dumping " + asset + "...");
                Wait(2000);
                PTFXDumpToXML.ConvertToXML();
                UI.Notify("PTFX Dump Done! Restart script with the INS key to see changes, or continue dumping more assets.");
                Wait(150);
            }
        }


        private void FxEvolution_OnItemLeftRight(UIMenu sender, UIMenuItem selectedItem, int index, bool left, UIMenuItem itemToControl, string fxName, string asset, bool useEvolutionArg, FxEvolution evo, string evolutionArg = "")
        {
            if (selectedItem == itemToControl)
            {
                evo.Amount = sender.ControlFloatValue(itemToControl, left, evo.Amount, 0.1f, 0.01f, 2, true);
                if (PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.SetEvolution(fxHandle, evolutionArg, evo.Amount);
                    PTFXHelper.SetAlpha(fxHandle, 1);
                }
            }
        }

        void Re_BuildFavouritesMenu()
        {
            foreach (var detailedPtfx in FavouritesList)
            {
                DecideDetailedFxMenuArrangement(FavouritesMenu, detailedPtfx);
            }
        }

        void SetupSearchMenu()
        {
            List<DetailedFx> searchFxList = new List<DetailedFx>();

            foreach (var dic in FxAssets)
            {
                foreach (var fxname in dic.FxNames.Where(n => n.PTFXName.Contains(searchString)))
                {
                    DetailedFx fx = new DetailedFx(dic.AssetName, fxname);
                    searchFxList.Add(fx);
                }
            }

            foreach (var detailedPtfx in searchFxList)
            {
                DecideDetailedFxMenuArrangement(SearchMenu, detailedPtfx);
            }
        }

        void DecideDetailedFxMenuArrangement(UIMenu menuToModify, DetailedFx detailedPtfx)
        {
            int evoListCount = detailedPtfx.FxName.EvolutionList.Count;
            if (evoListCount > 1)
            {
                UIMenu fxNameMenu = new UIMenu(detailedPtfx.FxName.PTFXName);
                _menuPool.AddSubMenu(fxNameMenu, menuToModify, detailedPtfx.FxName.PTFXName, "Asset Name: " + detailedPtfx.AssetName + ". May Require Evolution Arguments.");

                foreach (var e in detailedPtfx.FxName.EvolutionList)
                {
                    UIMenuItem evoName = new UIMenuItem("Evolution Name: " + e.EvolutionName, "< " + e.Amount + " >", "Hold SHIFT and select to save to/remove from favourites.");
                    fxNameMenu.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, evoName, detailedPtfx.FxName, detailedPtfx.AssetName, true);
                    fxNameMenu.OnItemLeftRight += (s, selItem, selIndex, direction) => FxEvolution_OnItemLeftRight(s, selItem, selIndex, direction, evoName, detailedPtfx.FxName.PTFXName, detailedPtfx.AssetName, true, e, e.EvolutionName);
                    fxNameMenu.AddMenuItem(evoName);
                }
            }
            else
            {
                UIMenuItem fxnameItem = new UIMenuItem(detailedPtfx.FxName.PTFXName, null, "Asset Name: " + detailedPtfx.AssetName);
                if (evoListCount == 1)
                {
                    fxnameItem.Value = "< " + detailedPtfx.FxName.EvolutionList[0].Amount + " >";
                    fxnameItem.Description = "Asset Name: " + detailedPtfx.AssetName +  ". Has one Evolution argument: " + detailedPtfx.FxName.EvolutionList[0].EvolutionName + ". Hold SHIFT and select to save to / remove from favourites.";
                    menuToModify.OnItemLeftRight += (s, selItem, selIndex, direction) => FxEvolution_OnItemLeftRight(s, selItem, selIndex, direction, fxnameItem, detailedPtfx.FxName.PTFXName, detailedPtfx.AssetName, true, detailedPtfx.FxName.EvolutionList[0], detailedPtfx.FxName.EvolutionList[0].EvolutionName);
                    menuToModify.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, fxnameItem, detailedPtfx.FxName, detailedPtfx.AssetName, true);
                }
                else
                {
                    fxnameItem.Description = "Asset Name: " + detailedPtfx.AssetName + ". No Evolution arguments found. Hold SHIFT and select to save to / remove from favourites.";
                    menuToModify.OnItemSelect += (s, selItem, selIndex) => FxHighlight_OnItemSelect(s, selItem, selIndex, fxnameItem, detailedPtfx.FxName, detailedPtfx.AssetName);
                }

                menuToModify.AddMenuItem(fxnameItem);
            }
        }

        void CreateSCMOD()
        {
            if (!File.Exists(@"scripts\ParticleFxPlayer.scmod"))
            {
                using (StreamWriter writer = new StreamWriter(@"scripts\ParticleFxPlayer.scmod"))
                {
                    writer.WriteLine("Particle Fx Player");
                    writer.WriteLine("Dev tool");
                }
            }
        }

        void CreateFavouritesXML()
        {
            if (!File.Exists(@"scripts\ParticleFxFavourites.xml"))
            {
                ForceDecimal();
                XMLHelper.SaveObjectToXML(FavouritesList, @"scripts\ParticleFxFavourites.xml");
            }
        }

        void CreateIni()
        {
            if (!File.Exists(@"scripts\ParticleFxPlayer.ini"))
            {

                ScriptSettings settings = ScriptSettings.Load(@"scripts\ParticleFxPlayer.ini");
                
                settings.SetValue("Control", "Menu Key", Keys.ShiftKey);
                settings.SetValue("Control", "Menu Key 2", Keys.N);

                settings.Save();
            }
        }

        void ReadINI()
        {
            ScriptSettings settings = ScriptSettings.Load(@"scripts\ParticleFxPlayer.ini");

            MenuKey = settings.GetValue("Control", "Menu Key", Keys.ShiftKey);
            MenuKey2 = settings.GetValue("Control", "Menu Key 2", Keys.N);
        }

        void ReadSCHelperINI()
        {
            string filepath = @"scripts\ScriptCommunicator.ini";

            if (File.Exists(@"scripts\ScriptCommunicator.dll"))
            {
                ScriptSettings config = ScriptSettings.Load(filepath);

                MenuKey = config.GetValue<Keys>("Keyboard Controls", "Menu Toggle Key 1", Keys.ShiftKey);
                MenuKey2 = config.GetValue<Keys>("Keyboard Controls", "Menu Toggle Key 2", Keys.N);
                buttonToggle1 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 1", Control.VehicleHandbrake);
                buttonToggle2 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 2", Control.VehicleHandbrake);
                buttonToggle3 = config.GetValue<Control>("Gamepad Controls", "Menu Toggle Button 3", Control.VehicleHorn);
            }
        }

        void OnTick(object sender, EventArgs e) // This is where most of your script goes
        {
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("ptfx_menu"))) // _HAS_CHEAT_STRING_JUST_BEEN_ENTERED
            {
                _menuPool.OpenCloseLastMenu();
                Wait(150);
            }

            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("ptfx_dump"))) // _HAS_CHEAT_STRING_JUST_BEEN_ENTERED
            {
                PTFXDumpToXML.ConvertToXML();
                UI.Notify("PTFX Dump Done! Restart script with the INS key.");
                Wait(150);
            }

            if (PtfxPlayerComm.IsEventTriggered())
            {
                _menuPool.OpenCloseLastMenu();

                PtfxPlayerComm.BlockScriptCommunicatorModMenu();
                PtfxPlayerComm.ResetEvent();
                Wait(300);
            }

            if (!allowMenuDraw)
            {
                //AdjustMenu();

                if (doSearch) { SetupSearchMenu(); doSearch = false; }
                if (rebuildFavourites)
                {
                    FavouritesMenu.ResetIndexPosition();
                    FavouritesMenu.SelectedItem = null;
                    FavouritesMenu.UIMenuItemList.Clear();
                    Re_BuildFavouritesMenu();
                    FavouritesMenu.SetIndexPosition(favLastIndex <= FavouritesMenu.UIMenuItemList.Count - 1 ? favLastIndex : FavouritesMenu.UIMenuItemList.Count - 1);
                    rebuildFavourites = false;
                }

                allowMenuDraw = true;
            }
            if (allowMenuDraw)
            {
                _menuPool.ProcessMenus();

                if (JustPressedMenuControls())
                {
                    if (_menuPool.IsAnyMenuOpen())
                    {
                        _menuPool.CloseAllMenus();
                    }
                    else
                    {
                        if (!SCHelperExists)
                        {
                            _menuPool.LastUsedMenu.IsVisible = !_menuPool.LastUsedMenu.IsVisible;
                        }
                    }

                    InputTimer = DateTime.Now.AddMilliseconds(300);
                }

                if (_menuPool.LastUsedMenu.IsVisible)
                {
                    PtfxPlayerComm.BlockScriptCommunicatorModMenu();
                }
                else
                {
                    if (PtfxPlayerComm.ScriptCommunicatorMenuIsBlocked())
                    {
                        //Wait(300);
                        PtfxPlayerComm.UnblockScriptCommunicatorModMenu();
                    }
                }
            }
        }

        bool JustPressedMenuControls()
        {
            if (InputTimer < DateTime.Now)
            {
                if (Game.CurrentInputMode == InputMode.MouseAndKeyboard)
                {
                    if (Game.IsKeyPressed(MenuKey) && Game.IsKeyPressed(MenuKey2))
                    {
                        return true;
                    }
                }
                else
                {
                    if (Game.IsControlPressed(2, buttonToggle1) && Game.IsControlPressed(2, buttonToggle2) && Game.IsControlPressed(2, buttonToggle3))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void PlayLoopOrNonLoopedFX(Entity e, string asset, FxName fxName, bool useEvolutionArgument)
        {
            if (!PTFXHelper.HasNamedPTFXAssetLoaded(asset))
            {
                PTFXHelper.RequestNamedPTFXAsset(asset);
                UI.ShowSubtitle("~r~ PTFX asset is not loaded, please try again ~n~ PTFX asset may not exist..");
            }
            
            if (PTFXHelper.HasNamedPTFXAssetLoaded(asset))
            {
                string fxType;
                Vector3 pos = e.Position + e.ForwardVector * 2f + e.RightVector * 1.5f;

                fxHandle = PTFXHelper.SpawnPTFXOnEntity(asset, fxName.PTFXName, e, new Vector3(1.5f, 2f, 0f), new Vector3(0.0f, 0.0f, 180.0f), fxSize);
                fxType = "Looped on Entity";

                /*if (!PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    fxHandle = PTFXHelper.SpawnPTFXOnEntityBone(asset, fxname, e, default(Vector3), default(Vector3), (int)Bone.IK_R_Hand, fxSize);
                    fxType = "Looped on Entity Bone";
                }

                if (!PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    fxHandle = PTFXHelper.SpawnPTFXOnPedBone(asset, fxname, (Ped)e, default(Vector3), default(Vector3), (int)Bone.IK_R_Hand, fxSize);
                    fxType = "Looped on Ped Bone";
                }*/

                if (!PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    fxHandle = PTFXHelper.SpawnPTFXAtCoordinate(asset, fxName.PTFXName, pos, new Vector3(0, 0, 180.0f), fxSize);
                    fxType = "Looped on Coordinate";
                }

                if (!PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    PTFXHelper.StartPTFXOnEntity(asset, fxName.PTFXName, e, new Vector3(1.5f, 2f, 0f), new Vector3(0.0f, 0.0f, 180.0f), fxSize);
                    PTFXHelper.StartPTFXAtCoordinate(asset, fxName.PTFXName, pos, new Vector3(0, 0, 180.0f), fxSize);
                    fxType = "Non Looped on Entity or on Coordinate, or doesn't work :(";
                }

                UI.ShowSubtitle("PTFX Spawn Type: " + fxType);
                
                if (useEvolutionArgument && PTFXHelper.DoesPTFXLoopedExist(fxHandle))
                {
                    foreach (var evo in fxName.EvolutionList)
                    {
                        PTFXHelper.SetEvolution(fxHandle, evo.EvolutionName, evo.Amount);
                        PTFXHelper.SetAlpha(fxHandle, 1);
                    }
                }
            }
        }

        void ForceRemoveSurroundingFX(Entity e)
        {
            PTFXHelper.RemovePTFX(fxHandle);
            PTFXHelper.RemovePTFXInRange(e.Position, 10f);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
        }

        void OnKeyUp(object sender, KeyEventArgs e)
        {
            /*if (e.KeyCode == MenuKey && Game.IsKeyPressed(Keys.ShiftKey) && !SCHelperExists)
            {
                _menuPool.OpenCloseLastMenu();
            }*/

            /*if (e.KeyCode == Keys.K)
            {
                PTFXDumpToXML.ConvertToXML();
                UI.ShowSubtitle("PTFX Dump Done! Restart script with the INS key.");
            }*/
        }
    }
}