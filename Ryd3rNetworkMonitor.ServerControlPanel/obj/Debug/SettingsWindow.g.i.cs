﻿#pragma checksum "..\..\SettingsWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "81FC376D90093909529DF4DAF339B9D64427CFC766153F53FA19A344AED3AB8A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ryd3rNetworkMonitor.ServerControlPanel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Ryd3rNetworkMonitor.ServerControlPanel {
    
    
    /// <summary>
    /// SettingsWindow
    /// </summary>
    public partial class SettingsWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 25 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label serverIpLbl;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label regPortLbl;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label mesPortLbl;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ipTxt;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox regPortTxt;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox mesPortTxt;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button cancelBtn;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\SettingsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button applyBtn;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Ryd3rNetworkMonitor.ServerControlPanel;component/settingswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\SettingsWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.serverIpLbl = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.regPortLbl = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.mesPortLbl = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.ipTxt = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.regPortTxt = ((System.Windows.Controls.TextBox)(target));
            
            #line 29 "..\..\SettingsWindow.xaml"
            this.regPortTxt.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.regPortTxt_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 6:
            this.mesPortTxt = ((System.Windows.Controls.TextBox)(target));
            
            #line 30 "..\..\SettingsWindow.xaml"
            this.mesPortTxt.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.mesPortTxt_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 7:
            this.cancelBtn = ((System.Windows.Controls.Button)(target));
            return;
            case 8:
            this.applyBtn = ((System.Windows.Controls.Button)(target));
            
            #line 33 "..\..\SettingsWindow.xaml"
            this.applyBtn.Click += new System.Windows.RoutedEventHandler(this.applyBtn_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

