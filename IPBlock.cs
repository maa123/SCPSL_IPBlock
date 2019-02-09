using Smod.IpBlock;

using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Events;
using Smod2.EventHandlers;

using System;
using System.Net;
using System.IO;

namespace IPBlock{
    [PluginDetails(
        author = "maa123",
        name = "IPBlock",
        description = "IPアドレスから特定の国以外の接続を拒否する",
        id = "maa123.IPBlock",
        version = "1.4",
        SmodMajor = 3,
        SmodMinor = 2,
        SmodRevision = 2
    )]
    class IPBlock : Plugin{
        public override void OnDisable(){
        }
        public override void OnEnable(){
            this.AddConfig(new Smod2.Config.ConfigSetting("ipblock_api_key", "0", Smod2.Config.SettingType.STRING, true, "APIKey"));
            this.Info("IP Blockが読み込まれました");
        }

        public override void Register(){
            this.AddEventHandlers(new PlEventHandler(this));
        }
    }
}

namespace Smod.IpBlock{
    class PlEventHandler : IEventHandlerPlayerJoin {
        private Plugin plugin;
        private string version = "0";
        private string api_id;

        public PlEventHandler(Plugin plugin){
            this.plugin = plugin;
            string[] rs=this.CheckAddr("::ffff:127.0.0.1");
            this.plugin.Info(rs[0]);
            this.plugin.Info(rs[1]);
        }
        public void OnPlayerJoin(PlayerJoinEvent ev){
            this.plugin.Info((string)ev.Player.IpAddress);
            this.api_id = ConfigManager.Manager.Config.GetStringValue("ipblock_api_key","0");
            this.plugin.Info(this.api_id);
            if(ev.Player.IpAddress=="localClient"){
                this.plugin.Info("IPアドレスの確認がスキップされました localhost");
            }else if(ev.Player.GetAuthToken().Contains("Bypass geo restrictions: YES")){
                this.plugin.Info("IPアドレスの確認がスキップされました Geo Bypass");
            }else{
                string[] result = this.CheckAddr((string)ev.Player.IpAddress,this.api_id);
                if(result[0] == "ok"){
                    this.plugin.Info("接続が許可されました");
                }else{
                    this.plugin.Info("接続が拒否されました");
                    ev.Player.Disconnect();
                }
                if("" != result[1]){
                    this.plugin.Info(result[1]);
                }
            }
        }
        private string[] CheckAddr(string ipaddr,string api_id = "0"){
            WebClient client = new WebClient ();
            client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead("http://tool.maa123.net/slipblock/ip_check?V="+this.version+"&api_id="+Uri.EscapeUriString(api_id)+"&IP="+Uri.EscapeUriString(ipaddr));
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            string[] result = s.Split(',');
            data.Close();
            reader.Close();
            return result;
        }
    }
}
