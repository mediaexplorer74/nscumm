﻿// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace NScumm.Mobile.Resx {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppResources {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("NScumm.Mobile.Resx.AppResources", typeof(AppResources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string DialogBox_OK {
            get {
                return ResourceManager.GetString("DialogBox.OK", resourceCulture);
            }
        }
        
        public static string DialogBox_Cancel {
            get {
                return ResourceManager.GetString("DialogBox.Cancel", resourceCulture);
            }
        }
        
        public static string GameLibrary_Title {
            get {
                return ResourceManager.GetString("GameLibrary.Title", resourceCulture);
            }
        }
        
        public static string RemoveGame_Title {
            get {
                return ResourceManager.GetString("RemoveGame.Title", resourceCulture);
            }
        }
        
        public static string RemoveGame_Message {
            get {
                return ResourceManager.GetString("RemoveGame.Message", resourceCulture);
            }
        }
        
        public static string ButtonDelete_Text {
            get {
                return ResourceManager.GetString("ButtonDelete.Text", resourceCulture);
            }
        }
        
        public static string ButtonScan_Text {
            get {
                return ResourceManager.GetString("ButtonScan.Text", resourceCulture);
            }
        }
        
        public static string Error_NoSdcardTitle {
            get {
                return ResourceManager.GetString("Error.NoSdcardTitle", resourceCulture);
            }
        }
        
        public static string Error_NoSdcard {
            get {
                return ResourceManager.GetString("Error.NoSdcard", resourceCulture);
            }
        }
        
        public static string Error_NoGameSelected {
            get {
                return ResourceManager.GetString("Error.NoGameSelected", resourceCulture);
            }
        }
        
        public static string Error_GameNotSupported {
            get {
                return ResourceManager.GetString("Error.GameNotSupported", resourceCulture);
            }
        }
        
        public static string Quit {
            get {
                return ResourceManager.GetString("Quit", resourceCulture);
            }
        }
    }
}
