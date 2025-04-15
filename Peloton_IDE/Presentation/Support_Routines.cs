using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

using Windows.Devices.Geolocation;
using Windows.Storage;

using Style = Microsoft.UI.Xaml.Style;
using TabSettingJson = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;


namespace Peloton_IDE.Presentation
{
    public sealed partial class MainPage : Page
    {
        //private bool InFocusTabIsPrFile()
        //{
        //    CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
        //    CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
        //    if (navigationViewItem.IsNewFile) return false;
        //    return navigationViewItem.SavedFilePath.Path.ToUpperInvariant().EndsWith(".PR");
        //}
        //
        //private bool InFocusTabSettingIsDifferent<T>(string setting, T value)
        //{
        //    T? lhs = Type_3_GetInFocusTab<T>(setting);
        //    return $"{lhs}" != $"{value}";
        //}
        private void SetMenuText(Dictionary<string, string> selectedLanguage) // #StatusBar #InterfaceLanguage
        {
            menuBar.Items.ForEach(item =>
            {
                HandlePossibleAmpersandInMenuItem(selectedLanguage[item.Name], item);
                item.Items.ForEach(subitem =>
                {
                    if (selectedLanguage.TryGetValue(subitem.Name, out string? value))
                        HandlePossibleAmpersandInMenuItem(value, subitem);

                });
            });

            foreach ((string key, MenuFlyoutItem opt) keyControl in new List<(string, MenuFlyoutItem)>
            {
                ("mnuQuiet", mnuQuiet),
                ("mnuVerbose", mnuVerbose),
                ("mnuVerbosePauseOnExit", mnuVerbosePauseOnExit),
                ("mnu20Seconds", mnu20Seconds),
                ("mnu100Seconds", mnu100Seconds),
                ("mnu200Seconds", mnu200Seconds),
                ("mnu1000Seconds", mnu1000Seconds),
                ("mnuInfinite", mnuInfinite)
            })
            {
                HandlePossibleAmpersandInMenuItem(selectedLanguage[keyControl.key], keyControl.opt);
            }

            foreach ((string key, TabViewItem tvi) keyControl in new List<(string, TabViewItem)>
            {
                ("tabOutput", tabOutput ),
                ("tabError", tabError),
                ("tabHtml", tabHtml ),
                ("tabLogo", tabLogo),
            })
            {
                keyControl.tvi.Header = selectedLanguage[keyControl.key];
            }

            foreach ((string key, MenuFlyoutItem mfi) keyControl in new List<(string, MenuFlyoutItem)>
            {
                ("tabOutput", mnuOutput ),
                ("tabError", mnuError),
                ("tabHtml", mnuHTML ),
                ("tabLogo", mnuLogo),
                ("tabRTF",mnuRTF)

            })
            {
                keyControl.mfi.Text = selectedLanguage[keyControl.key];
            }

            mnuRendering.Text = selectedLanguage["sbRendering"];

            ToolTipService.SetToolTip(butNew, selectedLanguage["new.Tip"]);
            ToolTipService.SetToolTip(butOpen, selectedLanguage["open.Tip"]);
            ToolTipService.SetToolTip(butSave, selectedLanguage["save.Tip"]);
            ToolTipService.SetToolTip(butSaveAs, selectedLanguage["cmdSaveAs"]);
            // ToolTipService.SetToolTip(butClose, selectedLanguage["close.Tip"]);
            ToolTipService.SetToolTip(butCopy, selectedLanguage["copy.Tip"]);
            ToolTipService.SetToolTip(butCut, selectedLanguage["cut.Tip"]);
            ToolTipService.SetToolTip(butPaste, selectedLanguage["paste.Tip"]);
            ToolTipService.SetToolTip(butSelectAll, selectedLanguage["mnuSelectOther(7)"]);
            ToolTipService.SetToolTip(butTransform, selectedLanguage["mnuTranslate"]);
            // ToolTipService.SetToolTip(toggleOutputButton, selectedLanguage["mnuToggleOutput"]);
            ToolTipService.SetToolTip(butGo, selectedLanguage["run.Tip"]);
        }
        private void HandleOutputPanelChange(string changeTo)
        {

            OutputPanelPosition outputPanelPosition = (OutputPanelPosition)Enum.Parse(typeof(OutputPanelPosition), changeTo);

            double outputPanelHeight = Type_1_GetVirtualRegistry<double>("ideOps.OutputPanelHeight");
            double outputPanelWidth = Type_1_GetVirtualRegistry<double>("ideOps.OutputPanelWidth");
            bool outputPanelShowing = Type_1_GetVirtualRegistry<bool>("ideOps.OutputPanelShowing");

            string outputPanelTabViewSettings = Type_1_GetVirtualRegistry<string>("OutputPanelTabView_Settings");
            string tabControlSettings = Type_1_GetVirtualRegistry<string>("TabControl_Settings");

            Telemetry.Disable();
            Telemetry.Transmit("outputPanelWidth=", outputPanelWidth, "outputPanelHeight=", outputPanelHeight, "outputPanelTabViewSettings=", outputPanelTabViewSettings, "tabControlSettings=", tabControlSettings, "outputPanel.ActualHeight=", outputPanel.ActualHeight, "outputPanel.ActualWidth=", outputPanel.ActualWidth, "App._window.Bounds=", App._window.Bounds);

            string optvPosition = FromBarredString_GetString(outputPanelTabViewSettings, 0);
            double optvHeight = FromBarredString_GetDouble(outputPanelTabViewSettings, 1);
            double optvWidth = FromBarredString_GetDouble(outputPanelTabViewSettings, 2);

            string tcPosition = FromBarredString_GetString(tabControlSettings, 0);
            double tcHeight = FromBarredString_GetDouble(tabControlSettings, 1);
            double tcWidth = FromBarredString_GetDouble(tabControlSettings, 2);

            switch (outputPanelPosition)
            {
                case OutputPanelPosition.Left:

                    Type_1_UpdateVirtualRegistry<string>("ideOps.OutputPanelPosition", OutputPanelPosition.Left.ToString());
                    RelativePanel.SetAlignLeftWithPanel(outputPanel, true);
                    RelativePanel.SetAlignRightWithPanel(outputPanel, false);
                    RelativePanel.SetBelow(outputPanel, butNew);


                    outputPanel.Height = outputPanelHeight; // Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelHeight") ?? 200.0;
                    outputPanel.Width = outputPanelWidth; //  Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelWidth") ?? 400.0;


                    outputPanel.MinWidth = 175;

                    outputPanel.ClearValue(HeightProperty);
                    outputPanel.ClearValue(MaxHeightProperty);

                    RelativePanel.SetAbove(tabControl, statusBar);
                    RelativePanel.SetRightOf(tabControl, outputPanel);
                    RelativePanel.SetAlignLeftWithPanel(tabControl, false);
                    RelativePanel.SetAlignRightWithPanel(tabControl, true);

                    outputLeftButton.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
                    outputBottomButton.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    outputRightButton.BorderBrush = new SolidColorBrush(Colors.LightGray);

                    outputLeftButton.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                    outputBottomButton.Background = new SolidColorBrush(Colors.Transparent);
                    outputRightButton.Background = new SolidColorBrush(Colors.Transparent);

                    Canvas.SetLeft(outputThumb, outputPanel.Width - 1);
                    Canvas.SetTop(outputThumb, 0);

                    Type_1_UpdateVirtualRegistry("ideOps.OutputPanelWidth", outputPanel.Width);

                    outputDockingFlyout.Hide();

                    break;
                case OutputPanelPosition.Bottom:
                    //outputPanelPosition = OutputPanelPosition.Bottom;
                    Type_1_UpdateVirtualRegistry<string>("ideOps.OutputPanelPosition", OutputPanelPosition.Bottom.ToString());
                    RelativePanel.SetAlignLeftWithPanel(tabControl, true);
                    RelativePanel.SetAlignRightWithPanel(tabControl, true);
                    RelativePanel.SetRightOf(tabControl, null);
                    RelativePanel.SetAbove(tabControl, outputPanel);

                    RelativePanel.SetAlignLeftWithPanel(outputPanel, true);
                    RelativePanel.SetAlignRightWithPanel(outputPanel, true);
                    RelativePanel.SetBelow(outputPanel, null);

                    outputPanel.Height = outputPanelHeight; // Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelHeight") ?? 200.0;
                    outputPanel.Width = outputPanelWidth; // Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelWidth") ?? 400.0;

                    outputPanel.MinHeight = 100;
                    outputPanel.ClearValue(WidthProperty);
                    outputPanel.ClearValue(MaxWidthProperty);

                    outputBottomButton.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
                    outputLeftButton.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    outputRightButton.BorderBrush = new SolidColorBrush(Colors.LightGray);

                    outputBottomButton.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                    outputLeftButton.Background = new SolidColorBrush(Colors.Transparent);
                    outputRightButton.Background = new SolidColorBrush(Colors.Transparent);

                    Canvas.SetLeft(outputThumb, 0);
                    Canvas.SetTop(outputThumb, -4);

                    Type_1_UpdateVirtualRegistry("ideOps.OutputPanelHeight", outputPanel.Height);

                    outputDockingFlyout.Hide();
                    break;
                case OutputPanelPosition.Right:
                    Type_1_UpdateVirtualRegistry<string>("ideOps.OutputPanelPosition", OutputPanelPosition.Right.ToString());
                    RelativePanel.SetAlignLeftWithPanel(outputPanel, false);
                    RelativePanel.SetAlignRightWithPanel(outputPanel, true);
                    RelativePanel.SetBelow(outputPanel, butNew);

                    outputPanel.Height = outputPanelHeight;// Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelHeight") ?? 200.0;
                    outputPanel.Width = outputPanelWidth; // Type_1_GetVirtualRegistry<double?>("ideOps.OutputPanelWidth") ?? 400.0;

                    outputPanel.MinWidth = 175;
                    outputPanel.ClearValue(HeightProperty);
                    outputPanel.ClearValue(MaxHeightProperty);

                    RelativePanel.SetAbove(tabControl, statusBar);
                    RelativePanel.SetLeftOf(tabControl, outputPanel);
                    RelativePanel.SetAlignLeftWithPanel(tabControl, true);
                    RelativePanel.SetAlignRightWithPanel(tabControl, false);

                    outputRightButton.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
                    outputBottomButton.BorderBrush = new SolidColorBrush(Colors.LightGray);
                    outputLeftButton.BorderBrush = new SolidColorBrush(Colors.LightGray);

                    outputRightButton.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                    outputBottomButton.Background = new SolidColorBrush(Colors.Transparent);
                    outputLeftButton.Background = new SolidColorBrush(Colors.Transparent);

                    Canvas.SetLeft(outputThumb, -4);
                    Canvas.SetTop(outputThumb, 0);

                    outputDockingFlyout.Hide();

                    Type_1_UpdateVirtualRegistry("ideOps.OutputPanelWidth", outputPanel.Width);

                    break;
            }

        }
        //private void ChangeHighlightOfMenuBarForLanguage(MenuBarItem mnuRun, string InterpreterLanguageName)
        //{
        //    Telemetry.Disable();

        //    Telemetry.Transmit("InterpreterLanguageName=", InterpreterLanguageName);
        //    IEnumerable<MenuFlyoutItemBase> subMenus = from menu in mnuRun.Items where menu.Name == "mnuLanguage" select menu;
        //    Telemetry.Transmit("subMenus.Any()=", subMenus.Any());
        //    if (subMenus.Any())
        //    {
        //        MenuFlyoutItemBase first = subMenus.First();

        //        foreach (MenuFlyoutItemBase? item in ((MenuFlyoutSubItem)first).Items)
        //        {
        //            Telemetry.Transmit("item.Name=", item.Name, "InterpreterLanguageName=", InterpreterLanguageName);
        //            if (item.Name == InterpreterLanguageName)
        //            {
        //                item.Foreground = new SolidColorBrush(Colors.White);
        //                item.Background = new SolidColorBrush(Colors.Black);
        //            }
        //            else
        //            {
        //                item.Foreground = new SolidColorBrush(Colors.Black);
        //                item.Background = new SolidColorBrush(Colors.White);
        //            }
        //        }
        //    }
        //}
        static List<PlexBlock>? GetAllPlexBlocks()
        {
            //IReadOnlyDictionary<string, ApplicationDataContainer> folder = ApplicationData.Current.LocalSettings.Containers;
            Telemetry.Disable();

            //string path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\Assets\Lexers";
            string path = @"c:\peloton\bin\lexers";
            List<PlexBlock> list = [];
            foreach (string file in Directory.GetFiles(path, "*.lex"))
            {
                Telemetry.Transmit("Lexer=", file);
                byte[] data = File.ReadAllBytes(file);
                using MemoryStream stream = new(data);
                using BsonDataReader reader = new(stream);
                JsonSerializer serializer = new();
                Plex? p = serializer.Deserialize<Plex>(reader);
                PlexBlock pb = new() { Plex = p, PlexFile = file };
                list.Add(pb!);
            }
            Telemetry.Disable();
            return list;
        }
        private bool AnInFocusTabExists()
        {
            return _richEditBoxes.Count > 0;
        }
        private CustomTabItem? InFocusTab()
        {
            if (AnInFocusTabExists())
            {
                return (CustomTabItem)tabControl.SelectedItem;
            }
            else
            {
                return null;
            }
        }
        #region Getters
        //private bool Type_1_ExistsVirtualRegistry(string name) => ApplicationData.Current.LocalSettings.Values.ContainsKey(name);
        private T Type_1_GetVirtualRegistry<T>(string name)
        {
            Telemetry.Disable();
            object result = ApplicationData.Current.LocalSettings.Values[name];
            Telemetry.Transmit(name + "=", name, "result=", result);
            return (T)result;
        }
        private T? Type_2_GetPerTabSettings<T>(string name)
        {
            Telemetry.Disable();
            return (bool)PerTabInterpreterParameters[name]["Defined"] ? (T?)(T)PerTabInterpreterParameters[name]["Value"] : default;
        }
        private T? Type_3_GetInFocusTab<T>(string name)
        {
            Telemetry.Disable();
            T? result = default;
            if (!AnInFocusTabExists())
            {
                return result;
            }
            CustomTabItem? inFocusTab = InFocusTab();
            if (inFocusTab != null)
            {
                if (inFocusTab.TabSettingsDict != null)
                {
                    if (inFocusTab.TabSettingsDict.ContainsKey(name))
                    {
                        if ((bool)inFocusTab.TabSettingsDict[name]["Defined"])
                        {
                            result = (T)inFocusTab.TabSettingsDict[name]["Value"];
                        }
                    }
                }
            }

            return result;
        }
        #endregion
        #region Setters
        // 1. virt reg
        private void Type_1_UpdateVirtualRegistry<T>(string name, T value)
        {
            Telemetry.Disable();
            Telemetry.Transmit(name, value);
            ApplicationData.Current.LocalSettings.Values[name] = value;
        }
        // 2. pertab
        private void Type_2_UpdatePerTabSettings<T>(string name, bool enabled, T value)
        {
            Telemetry.Disable();
            Telemetry.Transmit(name, enabled, value);
            PerTabInterpreterParameters[name]["Defined"] = enabled;
            PerTabInterpreterParameters[name]["Value"] = value!;
        }
        // 3. currtab
        private void Type_3_UpdateInFocusTabSettings<T>(string name, bool enabled, T value)
        {
            Telemetry.Disable();
            Telemetry.Transmit(name, enabled, value);
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            if (navigationViewItem == null || navigationViewItem.TabSettingsDict == null)
            {
                return;
            }
            navigationViewItem.TabSettingsDict[name]["Defined"] = enabled;
            navigationViewItem.TabSettingsDict[name]["Value"] = value!;
        }
        private async Task Type_3_UpdateInFocusTabSettingsIfPermittedAsync<T>(string name, bool defined, T value, string prompt)
        {
            Telemetry.Disable();
            Telemetry.Transmit(name, defined, value);
            CustomTabItem? inFocusTab = InFocusTab();
            if (inFocusTab == null || inFocusTab.TabSettingsDict == null)
            {
                return;
            }
            bool currentDefined = (bool)inFocusTab.TabSettingsDict[name]["Defined"];
            T currentValue = (T)inFocusTab.TabSettingsDict[name]["Value"];
            if (currentDefined == defined && $"{currentValue}" == $"{value}")
            {
                return;
            }
            if (await ChangingSettingsAllowed(prompt))
            {
                inFocusTab.TabSettingsDict[name]["Defined"] = defined;
                inFocusTab.TabSettingsDict[name]["Value"] = value!;
                UpdateStatusBar();
            }
        }
        private async Task<bool> ChangingSettingsAllowed(string prompt)
        {
            string il = Type_1_GetVirtualRegistry<string>("ideOps.InterfaceLanguageName");
            Dictionary<string, string> global = LanguageSettings[il]["GLOBAL"];

            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = prompt,
                PrimaryButtonText = global["1209"],
                SecondaryButtonText = global["1207"]
            };
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Secondary) { return true; }
            if (result == ContentDialogResult.Primary) { return false; }
            return false;
        }
        #endregion
        private void IfNotInVirtualRegistryUpdateItFromFactorySettingsOrDefaultTo<T>(string name, Dictionary<string, object>? factory, T defaultValue)
        {
            if (LocalSettings.Values.ContainsKey(name)) return;
            if (factory.TryGetValue(name, out object? factoryValue))
            {
                LocalSettings.Values[name] = factoryValue;
                return;
            }
            LocalSettings.Values[name] = (defaultValue.GetType().BaseType.Name == "Enum") ? defaultValue.ToString() : defaultValue;
            return;
        }
        private void SerializeTabsToVirtualRegistry()
        {
            Telemetry.Disable();
            string list = string.Join(',', outputPanelTabView.TabItems.Select(e =>
            {
                TabViewItem f = (TabViewItem)e;
                return (f.IsSelected ? "*" : "") + f.Name;
            }));
            Telemetry.Transmit(list);
            Type_1_UpdateVirtualRegistry<string>("TabViewLayout", list);
        }
        private void DeserializeTabsFromVirtualRegistry()
        {
            Telemetry.Disable();

            string? tabViewLayout = Type_1_GetVirtualRegistry<string>("TabViewLayout");
            if (tabViewLayout == null) return;

            string frontMost = "";
            tabViewLayout.Split(',').ForEach(key =>
            {
                if (key.StartsWith("*"))
                {
                    key = key[1..];
                    frontMost = key;
                    Telemetry.Transmit("frontMost=", frontMost);
                }
                TabViewItem found = (TabViewItem)outputPanelTabView.FindName(key);
                //found.IsSelected = false;
                outputPanelTabView.TabItems.Remove(found);
                outputPanelTabView.TabItems.Add(found);
                Telemetry.Transmit("Remove/Add=", found.Name);

            });
            if (frontMost.Length > 0)
            {
                TabViewItem found = (TabViewItem)outputPanelTabView.FindName(frontMost);
                outputPanelTabView.SelectedItem = found;
                Telemetry.Transmit(found.Name, "is selected item");
            }
        }
        //private void UpdateTopMostRendererInCurrentTab()
        //{
        //    Telemetry.Disable();

        //    if (!AnInFocusTabExists()) return;
        //    string? rendering = Type_3_GetInFocusTab<string>("outputOps.ActiveRenderers");
        //    long rend = Type_3_GetInFocusTab<long>("outputOps.TappedRenderer");
        //    if (rendering == null || rendering.Split(',', StringSplitOptions.RemoveEmptyEntries).Length == 0)
        //    {
        //        return;
        //    }
        //    Telemetry.Transmit("rend=", rend);
        //    foreach (TabViewItem tvi in outputPanelTabView.TabItems)
        //    {
        //        if (rend != long.Parse((string)tvi.Tag)) continue;
        //        tvi.IsSelected = true;
        //        break;
        //    }
        //}

        private async Task<bool> FileNotFoundDialog(string? file)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = $"File '{file}' not found",
                PrimaryButtonText = "OK"
            };
            ContentDialogResult result = await dialog.ShowAsync();
            return false;
        }
        private async Task<bool> SomethingNotFoundDialog(string? something)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = something,
                PrimaryButtonText = "OK"
            };
            ContentDialogResult result = await dialog.ShowAsync();
            return false;
        }
        private async Task<bool> TestPresenceOfAllPlexes()
        {
            var plexes = GetAllPlexBlocks();
            if (plexes == null || plexes.Count == 0)
            {
                ContentDialog dialog = new()
                {
                    XamlRoot = this.XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "No BSON-formatted .lex files found in c:\\peloton\\bin\\lexers",
                    PrimaryButtonText = "OK"
                };
                ContentDialogResult result = await dialog.ShowAsync();
                return false;
            }

            return true;
        }
        private async Task<bool> AskToDownloadPowerShell()
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Download PowerShell? (Application will shut down)",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
            ContentDialogResult result = await dialog.ShowAsync();
            return (ContentDialogResult.Primary == result);
        }
        private void UpdateOutputTabs()
        {
            Telemetry.Disable();
            if (!AnInFocusTabExists()) return;

            DeselectAndDisableAllOutputPanelTabs();
            EnableAllOutputPanelTabsMatchingRendering();

            string? rendering = Type_3_GetInFocusTab<string>("outputOps.ActiveRenderers");
            if (rendering != null && rendering.Split(",", StringSplitOptions.RemoveEmptyEntries).Any())
            {
                var selectedRenderer = Type_3_GetInFocusTab<long>("outputOps.TappedRenderer");
                (from TabViewItem tvi in outputPanelTabView.TabItems where long.Parse((string)tvi.Tag) == selectedRenderer select tvi).ForEach(tvi =>
                {
                    tvi.IsSelected = true;
                    Telemetry.Transmit(tvi.Name, tvi.Tag, "frontmost");
                    Type_3_UpdateInFocusTabSettings<long>("outputOps.TappedRenderer", true, selectedRenderer);
                });
            }
        }
        private void UpdateStatusBar()
        {
            if (LanguageSettings == null) return;
            string interfaceLanguageName = Type_1_GetVirtualRegistry<string>("ideOps.InterfaceLanguageName");
            Dictionary<string, Dictionary<string, string>> selectedLanguage = LanguageSettings[interfaceLanguageName];

            Dictionary<string, string> global = LanguageSettings[interfaceLanguageName]["GLOBAL"];
            Dictionary<string, string> frmMain = LanguageSettings[interfaceLanguageName]["frmMain"];

            string[] quietudes = ["mnuQuiet", "mnuVerbose", "mnuVerbosePauseOnExit"];
            string[] timeouts = ["mnu20Seconds", "mnu100Seconds", "mnu200Seconds", "mnu1000Seconds", "mnuInfinite"];

            TabSettingJson? tabSettingsDict;
            long interp;
            bool isVariableLength;
            long quietude;
            long timeout;

            if (AnInFocusTabExists())
            {
                tabSettingsDict = InFocusTab().TabSettingsDict;
                sbLanguageName.Text = GetLanguageNameOfCurrentTab(tabSettingsDict);
                sbCommandLine.Text = BuildTabCommandLine();
                interp = Type_3_GetInFocusTab<long>("ideOps.Engine");
                isVariableLength = Type_3_GetInFocusTab<bool>("pOps.VariableLength");
                quietude = Type_3_GetInFocusTab<long>("pOps.Quietude");
                timeout = Type_3_GetInFocusTab<long>("ideOps.Timeout");
            }
            else
            {
                tabSettingsDict = PerTabInterpreterParameters;
                sbLanguageName.Text = GetLanguageNameOfCurrentTab(tabSettingsDict);
                sbCommandLine.Text = string.Empty;
                interp = Type_1_GetVirtualRegistry<long>("ideOps.Engine");
                isVariableLength = Type_1_GetVirtualRegistry<bool>("pOps.VariableLength");
                quietude = Type_1_GetVirtualRegistry<long>("pOps.Quietude");
                timeout = Type_1_GetVirtualRegistry<long>("ideOps.Timeout");
            }



            sbFixedVariable.Text = (isVariableLength ? "#" : "@") + global[isVariableLength ? "variableLength" : "fixedLength"];
            sbQuietude.Text = frmMain[quietudes.ElementAt((int)quietude)];
            sbTimeout.Text = $"{frmMain["mnuTimeout"]}: {frmMain[timeouts.ElementAt((int)timeout)]}";
            sbCommandLine.Text = BuildTabCommandLine();
            sbRendering.Text = frmMain["sbRendering"].ToString();

            // sbCursorPosition
            // sbEngine

            switch (interp)
            {
                case 2:
                    sbEngine.Text = "P2";
                    break;

                case 3:
                    sbEngine.Text = "P3";
                    break;
            }

            if (AnInFocusTabExists() && InFocusTab().SavedFileName != null)
                InFocusTab().Content = InFocusTab().SavedFileName;

            //string? savedFilePath = null;
            //if (AnInFocusTabExists())
            //{
            //    var inFocusTab = InFocusTab();
            //    if (inFocusTab.SavedFilePath != null)
            //    {
            //        savedFilePath = inFocusTab.SavedFileFolder;
            //    }
            //}

            //string mostRecentPickedFilePath = Type_1_GetVirtualRegistry<string>("MostRecentPickedFilePath");

            sbPath.Text = $"{frmMain["mnuCode"]}: {Type_3_GetInFocusTab<string>("ideOps.CodeFolder") ?? Type_1_GetVirtualRegistry<string>("ideOps.CodeFolder")}";
        }
        private void UpdateMenus() // NOTE this is all Type_1 stuff. We don't care what the Type_2 and Type_3 settings are
        {
            Telemetry.Disable();

            // mnuFormat mnuFontSize
            DoMnuFontSize();

            // mnuSource mnuVariableLength
            DoMnuVariableLength();

            // mnuRun mnuChooseEngine
            DoMnuChooseEngine();

            // mnuRun mnuLanguage
            DoMnuLanguage();

            // mnuRun mnuRunningMode
            DoMnuRunningMode();

            // mnuRun mnuTimeout
            DoMnuTimeout();

            // mnuRun mnuRendering
            DoMnuRendering();

            // mnuRun mnuTransput
            DoMnuTransput();

            // mnuSettings mnuTabCreationMethod
            DoMnuTabCreationMethod();

            // mnuSettings mnuSelectLanguage
            DoMnuSelectLanguage();

            void DoMnuVariableLength()
            {
                MenuItemHighlightController(mnuVariableLength, Type_1_GetVirtualRegistry<bool>("pOps.VariableLength"));
            }

            void DoMnuChooseEngine()
            {
                if (Type_1_GetVirtualRegistry<long>("ideOps.Engine") == 2L)
                {
                    MenuItemHighlightController(mnuNewEngine, false);
                    MenuItemHighlightController(mnuOldEngine, true);
                }
                else
                {
                    MenuItemHighlightController(mnuNewEngine, true);
                    MenuItemHighlightController(mnuOldEngine, false);
                }
            }

            void DoMnuLanguage()
            {
                string InterpreterLanguageName = Type_1_GetVirtualRegistry<string>("mainOps.InterpreterLanguageName");
                IEnumerable<MenuFlyoutItemBase> subMenus = from menu in mnuRun.Items where menu.Name == "mnuLanguage" select menu;
                Telemetry.Transmit("subMenus.Any()=", subMenus.Any());
                if (subMenus.Any())
                {
                    MenuFlyoutItemBase first = subMenus.First();

                    foreach (MenuFlyoutItemBase? item in ((MenuFlyoutSubItem)first).Items)
                    {
                        Telemetry.Transmit("item.Name=", item.Name, "InterpreterLanguageName=", InterpreterLanguageName);
                        if (item.Name == InterpreterLanguageName)
                        {
                            item.Foreground = new SolidColorBrush(Colors.White);
                            item.Background = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            item.Foreground = new SolidColorBrush(Colors.Black);
                            item.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                }
            }

            void DoMnuRunningMode()
            {
                long quietude = Type_1_GetVirtualRegistry<long>("pOps.Quietude");
                mnuRunningMode.Items.ForEach(item =>
                {
                    MenuItemHighlightController((MenuFlyoutItem)item, false);
                    if (quietude == long.Parse((string)item.Tag))
                    {
                        MenuItemHighlightController((MenuFlyoutItem)item, true);
                    }
                });
            }

            void DoMnuTimeout()
            {
                foreach (MenuFlyoutItemBase? item in mnuTimeout.Items)
                {
                    MenuItemHighlightController((MenuFlyoutItem)item!, false);
                }
                long currTimeout = Type_1_GetVirtualRegistry<long>("ideOps.Timeout");

                switch (currTimeout)
                {
                    case 0:
                        MenuItemHighlightController(mnu20Seconds, true);
                        break;

                    case 1:
                        MenuItemHighlightController(mnu100Seconds, true);
                        break;

                    case 2:
                        MenuItemHighlightController(mnu200Seconds, true);
                        break;

                    case 3:
                        MenuItemHighlightController(mnu1000Seconds, true);
                        break;

                    case 4:
                        MenuItemHighlightController(mnuInfinite, true);
                        break;

                }
            }

            void DoMnuRendering()
            {
                List<string> renderers = [.. Type_1_GetVirtualRegistry<string>("outputOps.ActiveRenderers").Split(',').Select(x => x.Trim())];

                mnuRendering.Items.ForEach(item =>
                {
                    MenuItemHighlightController((MenuFlyoutItem)item, false);
                    if (renderers.Contains((string)item.Tag))
                    {
                        MenuItemHighlightController((MenuFlyoutItem)item, true);
                    }

                });
            }

            void DoMnuTransput()
            {
                Telemetry.Disable();

                string transput = Type_1_GetVirtualRegistry<long>("pOps.Transput").ToString();
                foreach (var mfi in from MenuFlyoutSubItem mfsi in mnuTransput.Items.Cast<MenuFlyoutSubItem>()
                                    where mfsi != null
                                    where mfsi.Items.Count > 0
                                    from MenuFlyoutItem mfi in mfsi.Items.Cast<MenuFlyoutItem>()
                                    select mfi)
                {
                    MenuItemHighlightController((MenuFlyoutItem)mfi, false);
                    if (transput == (string)mfi.Tag)
                    {
                        MenuItemHighlightController((MenuFlyoutItem)mfi, true);
                    }
                }
            }

            void DoMnuTabCreationMethod()
            {
                bool UsePerTabSettingsWhenCreatingTab = Type_1_GetVirtualRegistry<bool>("ideOps.UsePerTabSettingsWhenCreatingTab");
                MenuItemHighlightController(mnuPerTabSettings, UsePerTabSettingsWhenCreatingTab);
                MenuItemHighlightController(mnuCurrentTabSettings, !UsePerTabSettingsWhenCreatingTab);
            }

            void DoMnuSelectLanguage()
            {
                long id = Type_1_GetVirtualRegistry<long>("ideOps.InterfaceLanguageID");
                mnuSelectLanguage.Items.ForEach(item =>
                {
                    MenuItemHighlightController((MenuFlyoutItem)item, item.Tag.ToString() == id.ToString());
                });
            }

            void DoMnuFontSize()
            {
                double fontsize = Type_1_GetVirtualRegistry<double>("ideOps.FontSize");
                mnuFontSize.Items.ForEach(item =>
                {
                    MenuItemHighlightController((MenuFlyoutItem)item, false);
                    if (fontsize == double.Parse((string)item.Tag))
                    {
                        MenuItemHighlightController((MenuFlyoutItem)item, true);
                    }
                });
            }
        }
        private bool FromBarredString_GetBoolean(string list, int entry)
        {
            string item = list.Split(['|'])[entry];
            return bool.Parse(item);
        }
        private string FromBarredString_GetString(string list, int entry)
        {
            string item = list.Split(['|'])[entry];
            return item;
        }
        private double FromBarredString_GetDouble(string list, int entry)
        {
            string item = list.Split(['|'])[entry];
            return double.Parse(item);
        }

        static void ExtractPelotonAssets(string tag = "")
        {
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            string zipPath = root + @"\Assets\InstallationItems\PelotonAssets.zip";
            if (!File.Exists(zipPath)) { return; }
            Directory.CreateDirectory(@"C:\Peloton");
            foreach (ZipArchiveEntry entry in ZipFile.OpenRead(zipPath).Entries)
            {
                string target;
                if (tag == "")
                {
                    target = $"C:/peloton/{entry.FullName}";
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    if (entry.Length > 0)
                        entry.ExtractToFile(target, true);
                }
                else
                {
                    if (entry.FullName.StartsWith(tag + "/"))
                    {
                        target = $"C:/peloton/{entry.FullName}";
                        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                        if (entry.Length > 0)
                            entry.ExtractToFile(target, true);
                    }
                }
            }
        }

        public static bool IsPowerShellInstalled()
        {   
            object? regTest = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore", "UseMU", null);
            string psTest = FileFolderPicking.PwSh(@"Get-ItemPropertyValue -Path 'HKLM:\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\*' -Name 'SemanticVersion'");
            if (regTest != null && psTest.Length > 0)
            {
                return true;
            }
            return false;
        }

        public void InsertCodeTemplate(string key)
        {
            bool VariableLength = Type_1_GetVirtualRegistry<bool>("pOps.VariableLength");
            CustomTabItem navigationViewItem = (CustomTabItem)tabControl.SelectedItem;
            CustomRichEditBox currentRichEditBox = _richEditBoxes[navigationViewItem.Tag];
            ITextSelection selection = currentRichEditBox.Document.Selection;
            if (selection != null)
            {
                selection.StartPosition = selection.EndPosition;
                switch (key)
                {
                    case "F2":
                        if (VariableLength)
                        {
                            selection.Text = "<# ></#>";
                        }
                        else
                        {
                            selection.Text = "<@ ></@>";
                        }
                        break;

                    case "F3":
                        if (VariableLength)
                        {
                            selection.Text = "<@ ></@>";
                        }
                        else
                        {
                            selection.Text = "<# ></#>";
                        }
                        break;
                }
                selection.EndPosition = selection.StartPosition;
                currentRichEditBox.Document.Selection.Move(TextRangeUnit.Character, 3);
            }
        }
        //public void SwitchToTab(int direction)
        //{
        //    if (_richEditBoxes.Count > 0)
        //    {
        //        foreach (KeyValuePair<object, CustomRichEditBox> _reb in _richEditBoxes)
        //        {
        //            if (_reb.Value.IsDirty)
        //            {
        //                object key = _reb.Key;
        //                CustomRichEditBox aRichEditBox = _richEditBoxes[key];
        //                foreach (object? item in tabControl.MenuItems)
        //                {
        //                    CustomTabItem? cti = item as CustomTabItem;
        //                    string content = cti.Content.ToString().Replace(" ", "");
        //                    if (content == key as string)
        //                    {
        //                        Debug.WriteLine(cti.Content);
        //                        cti.Focus(FocusState.Keyboard); // was Pointer
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
