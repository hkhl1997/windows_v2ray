

using ServiceLib.Models;
using Splat;

namespace ServiceLib.ViewModels
{
    public class MainWindowViewModel
    {
        protected static Config? _config;
        protected Func<EViewAction, object?, Task<bool>>? _updateView;

        private bool _hasNextReloadJob = false;

        public bool BlReloadEnabled = true;

        public ProfileItem CurrentNode { get; set; }

        #region Init

        public MainWindowViewModel(Func<EViewAction, object?, Task<bool>>? updateView)
        {
            _config = AppHandler.Instance.Config;
            _config.RoutingBasicItem.RoutingIndexId = "5235975504146572791";
            _updateView = updateView;

            _ = Init();
        }

        private async Task Init()
        {
            _config.UiItem.ShowInTaskbar = true;

            _config.SystemProxyItem.SysProxyType = ESysProxyType.ForcedChange;

            await ConfigHandler.InitBuiltinRouting(_config);
            await ConfigHandler.InitBuiltinDNS(_config);
            await ProfileExHandler.Instance.Init();
            await CoreHandler.Instance.Init(_config, UpdateHandler);
            TaskHandler.Instance.RegUpdateTask(_config, UpdateTaskHandler);

            if (_config.GuiItem.EnableStatistics || _config.GuiItem.DisplayRealTimeSpeed)
            {
                await StatisticsHandler.Instance.Init(_config, UpdateStatisticsHandler);
            }

            //BlReloadEnabled = true;
            await Reload();
            await AutoHideStartup();
            //Locator.Current.GetService<StatusBarViewModel>()?.RefreshRoutingsMenu();
        }

        public async Task ChangeRouteModelAsync(bool isGlobal)
        {
            var modelId = "5235975504146572791";

            if (isGlobal)
            {
                modelId = "5018099314562672586";
            }

            var item = await AppHandler.Instance.GetRoutingItem(modelId);
            if (item is null)
            {
                return;
            }
            if (_config.RoutingBasicItem.RoutingIndexId == item.Id)
            {
                return;
            }

            if (await ConfigHandler.SetDefaultRouting(_config, item) == 0)
            {
                //await Reload();
            }
        }

        #endregion Init

        #region Actions

        private void UpdateHandler(bool notify, string msg)
        {
            NoticeHandler.Instance.SendMessage(msg);
            if (notify)
            {
                NoticeHandler.Instance.Enqueue(msg);
            }
        }

        private void UpdateTaskHandler(bool success, string msg)
        {
            NoticeHandler.Instance.SendMessageEx(msg);
            if (success)
            {
                var indexIdOld = _config.IndexId;
                RefreshServers();
                if (indexIdOld != _config.IndexId)
                {
                    _ = Reload();
                }
                if (_config.UiItem.EnableAutoAdjustMainLvColWidth)
                {
                    _updateView?.Invoke(EViewAction.AdjustMainLvColWidth, null);
                }
            }
        }

        private void UpdateStatisticsHandler(ServerSpeedItem update)
        {
            if (!_config.UiItem.ShowInTaskbar)
            {
                return;
            }
            _updateView?.Invoke(EViewAction.DispatcherStatistics, update);
        }

        public void SetStatisticsResult(ServerSpeedItem update)
        {
            //if (_config.GuiItem.DisplayRealTimeSpeed)
            //{
            //    Locator.Current.GetService<StatusBarViewModel>()?.UpdateStatistics(update);
            //}
            //if (_config.GuiItem.EnableStatistics && (update.ProxyUp + update.ProxyDown) > 0 && DateTime.Now.Second % 9 == 0)
            //{
            //    Locator.Current.GetService<ProfilesViewModel>()?.UpdateStatistics(update);
            //}
        }

        public async Task MyAppExitAsync(bool blWindowsShutDown)
        {
            try
            {
                Logging.SaveLog("MyAppExitAsync Begin");

                await SysProxyHandler.UpdateSysProxy(_config, true);
                //MessageBus.Current.SendMessage("", EMsgCommand.AppExit.ToString());

                await ConfigHandler.SaveConfig(_config);
                await ProfileExHandler.Instance.SaveTo();
                await StatisticsHandler.Instance.SaveTo();
                await CoreHandler.Instance.CoreStop();
                StatisticsHandler.Instance.Close();

                Logging.SaveLog("MyAppExitAsync End");
            }
            catch { }
            finally
            {
                if (!blWindowsShutDown)
                {
                    _updateView?.Invoke(EViewAction.Shutdown, false);
                }
            }
        }

        public async Task UpgradeApp(string arg)
        {
            if (!Utils.UpgradeAppExists(out var upgradeFileName))
            {
                NoticeHandler.Instance.SendMessageAndEnqueue(ResUI.UpgradeAppNotExistTip);
                Logging.SaveLog("UpgradeApp does not exist");
                return;
            }

            var id = ProcUtils.ProcessStart(upgradeFileName, arg, Utils.StartupPath());
            if (id > 0)
            {
                await MyAppExitAsync(false);
            }
        }

        public void ShowHideWindow(bool? blShow)
        {
            _updateView?.Invoke(EViewAction.ShowHideWindow, blShow);
        }

        public void Shutdown(bool byUser)
        {
            _updateView?.Invoke(EViewAction.Shutdown, byUser);
        }

        #endregion Actions

        #region Servers && Groups

        private void RefreshServers()
        {
            //MessageBus.Current.SendMessage("", EMsgCommand.RefreshProfiles.ToString());
        }

        private void RefreshSubscriptions()
        {
            //Locator.Current.GetService<ProfilesViewModel>()?.RefreshSubscriptions();
        }

        #endregion Servers && Groups

        #region Add Servers

        public async Task AddServerAsync(bool blNew, EConfigType eConfigType)
        {
            ProfileItem item = new()
            {
                Subid = _config.SubIndexId,
                ConfigType = eConfigType,
                IsSub = false,
            };

            bool? ret = false;
            if (eConfigType == EConfigType.Custom)
            {
                ret = await _updateView?.Invoke(EViewAction.AddServer2Window, item);
            }
            else
            {
                ret = await _updateView?.Invoke(EViewAction.AddServerWindow, item);
            }
            if (ret == true)
            {
                RefreshServers();
                if (item.IndexId == _config.IndexId)
                {
                    await Reload();
                }
            }
        }

        public async Task AddServerViaClipboardAsync(string? clipboardData)
        {
            if (clipboardData == null)
            {
                await _updateView?.Invoke(EViewAction.AddServerViaClipboard, null);
                return;
            }
            int ret = await ConfigHandler.AddBatchServers(_config, clipboardData, _config.SubIndexId, false);
            if (ret > 0)
            {
                RefreshSubscriptions();
                RefreshServers();
                NoticeHandler.Instance.Enqueue(string.Format(ResUI.SuccessfullyImportedServerViaClipboard, ret));
            }
            else
            {
                NoticeHandler.Instance.Enqueue(ResUI.OperationFailed);
            }
        }

        public async Task AddServerViaScanAsync()
        {
            _updateView?.Invoke(EViewAction.ScanScreenTask, null);
            await Task.CompletedTask;
        }

        public async Task ScanScreenResult(byte[]? bytes)
        {
            var result = QRCodeHelper.ParseBarcode(bytes);
            await AddScanResultAsync(result);
        }

        public async Task AddServerViaImageAsync()
        {
            _updateView?.Invoke(EViewAction.ScanImageTask, null);
            await Task.CompletedTask;
        }

        public async Task ScanImageResult(string fileName)
        {
            if (Utils.IsNullOrEmpty(fileName))
            {
                return;
            }

            var result = QRCodeHelper.ParseBarcode(fileName);
            await AddScanResultAsync(result);
        }

        private async Task AddScanResultAsync(string? result)
        {
            if (Utils.IsNullOrEmpty(result))
            {
                NoticeHandler.Instance.Enqueue(ResUI.NoValidQRcodeFound);
            }
            else
            {
                int ret = await ConfigHandler.AddBatchServers(_config, result, _config.SubIndexId, false);
                if (ret > 0)
                {
                    RefreshSubscriptions();
                    RefreshServers();
                    NoticeHandler.Instance.Enqueue(ResUI.SuccessfullyImportedServerViaScan);
                }
                else
                {
                    NoticeHandler.Instance.Enqueue(ResUI.OperationFailed);
                }
            }
        }

        #endregion Add Servers

        #region Subscription

        private async Task SubSettingAsync()
        {
            if (await _updateView?.Invoke(EViewAction.SubSettingWindow, null) == true)
            {
                RefreshSubscriptions();
            }
        }

        public async Task UpdateSubscriptionProcess(string subId, bool blProxy)
        {
            await (new UpdateService()).UpdateSubscriptionProcess(_config, subId, blProxy, UpdateTaskHandler);
        }

        #endregion Subscription

        #region Setting

        private async Task OptionSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.OptionSettingWindow, null);
            if (ret == true)
            {
                //Locator.Current.GetService<StatusBarViewModel>()?.InboundDisplayStatus();
                await Reload();
            }
        }

        private async Task RoutingSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.RoutingSettingWindow, null);
            if (ret == true)
            {
                await ConfigHandler.InitBuiltinRouting(_config);
                //Locator.Current.GetService<StatusBarViewModel>()?.RefreshRoutingsMenu();
                await Reload();
            }
        }

        private async Task DNSSettingAsync()
        {
            var ret = await _updateView?.Invoke(EViewAction.DNSSettingWindow, null);
            if (ret == true)
            {
                await Reload();
            }
        }

        private bool AllowEnableTun()
        {
            if (Utils.IsWindows())
            {
                return AppHandler.Instance.IsAdministrator;
            }
            else if (Utils.IsLinux())
            {
                return _config.TunModeItem.LinuxSudoPwd.IsNotEmpty();
            }
            else if (Utils.IsOSX())
            {
                return _config.TunModeItem.LinuxSudoPwd.IsNotEmpty();
            }
            return false;
        }

        public async Task DoEnableTun(bool c)
        {
            _config.TunModeItem.EnableTun = c;
            // When running as a non-administrator, reboot to administrator mode
            if (c && AllowEnableTun() == false)
            {
                if (Utils.IsWindows())
                {
                    _config.TunModeItem.EnableTun = false;
                    RebootAsAdmin();
                    return;
                }
                else if (Utils.IsOSX())
                {
                    _config.TunModeItem.EnableTun = false;
                    NoticeHandler.Instance.SendMessageAndEnqueue(ResUI.TbSettingsLinuxSudoPasswordIsEmpty);
                    return;
                }
            }

            if(c)
            {
                _config.SystemProxyItem.SysProxyType = ESysProxyType.ForcedClear;
            }
            else
            {
                _config.SystemProxyItem.SysProxyType = ESysProxyType.ForcedChange;
            }
            
            await ConfigHandler.SaveConfig(_config);
        }

        public async Task RebootAsAdmin()
        {
            ProcUtils.RebootAsAdmin();
            await MyAppExitAsync(false);
        }

        private async Task ClearServerStatistics()
        {
            await StatisticsHandler.Instance.ClearAllServerStatistics();
            RefreshServers();
        }

        private async Task OpenTheFileLocation()
        {
            var path = Utils.StartupPath();
            if (Utils.IsWindows())
            {
                ProcUtils.ProcessStart(path);
            }
            else if (Utils.IsLinux())
            {
                ProcUtils.ProcessStart("nautilus", path);
            }
            else if (Utils.IsOSX())
            {
                ProcUtils.ProcessStart("open", path);
            }
            await Task.CompletedTask;
        }

        #endregion Setting

        #region core job

        public async Task Reload()
        {
            if (CurrentNode == null)
                return;

            //If there are unfinished reload job, marked with next job.
            //if (!BlReloadEnabled)
            //{
            //    _hasNextReloadJob = true;
            //    return;
            //}

            //BlReloadEnabled = false;

            await LoadCore();
            await SysProxyHandler.UpdateSysProxy(_config, false);
            //Locator.Current.GetService<StatusBarViewModel>()?.TestServerAvailability();

            _updateView?.Invoke(EViewAction.DispatcherReload, null);

            //BlReloadEnabled = true;
            //if (_hasNextReloadJob)
            //{
            //    _hasNextReloadJob = false;
            //    await Reload();
            //}
        }

        public void ReloadResult()
        {
            //// BlReloadEnabled = true;
            ////Locator.Current.GetService<StatusBarViewModel>()?.ChangeSystemProxyAsync(_config.systemProxyItem.sysProxyType, false);
            //ShowClashUI = _config.IsRunningCore(ECoreType.sing_box);
            //if (ShowClashUI)
            //{
            //    Locator.Current.GetService<ClashProxiesViewModel>()?.ProxiesReload();
            //}
            //else
            //{ TabMainSelectedIndex = 0; }
        }

        private async Task LoadCore()
        {
            ////var node = await ConfigHandler.GetDefaultServer(_config);

            //var node = new ProfileItem();

            ////node.Address = "53bdde59.flzxjxo53slwj3n.e7mbqhx.com";
            ////node.ConfigType = EConfigType.Shadowsocks;
            ////node.ConfigVersion = 2;
            ////node.DisplayLog = true;
            ////node.Id = "OwzVzPeKuRZm";
            ////node.IndexId = "1";
            ////node.IsSub = true;
            ////node.Port = 57654;
            ////node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
            ////node.Security = "chacha20-ietf-poly1305";
            ////node.Subid = "2";

            //node.Address = "bd59feed.flbgpi-tw.p2tib8n.com";
            //node.ConfigType = EConfigType.Shadowsocks;
            //node.ConfigVersion = 2;
            //node.DisplayLog = true;
            //node.Id = "OwzVzPeKuRZm";
            //node.Port = 57632;
            //node.Remarks = "动态BGP+IPLC|美国04(原生)|2x";
            //node.Security = "chacha20-ietf-poly1305";

            await CoreHandler.Instance.LoadCore(CurrentNode);
        }


        private async void ChangeProxy()
        {
            _config.SystemProxyItem.SysProxyType = ESysProxyType.ForcedChange;

            await SysProxyHandler.UpdateSysProxy(_config, false);

            await ConfigHandler.SaveConfig(_config);
        }

        public async Task CloseCore()
        {
            await ConfigHandler.SaveConfig(_config);
            await CoreHandler.Instance.CoreStop();
        }

        private async Task AutoHideStartup()
        {
            if (_config.UiItem.AutoHideStartup)
            {
                ShowHideWindow(false);
            }
            await Task.CompletedTask;
        }

        #endregion core job

        #region Presets

        public async Task ApplyRegionalPreset(EPresetType type)
        {
            await ConfigHandler.ApplyRegionalPreset(_config, type);
            await ConfigHandler.InitRouting(_config);
            //Locator.Current.GetService<StatusBarViewModel>()?.RefreshRoutingsMenu();

            await ConfigHandler.SaveConfig(_config);
            await new UpdateService().UpdateGeoFileAll(_config, UpdateHandler);
            await Reload();
        }

        #endregion Presets
    }
}
