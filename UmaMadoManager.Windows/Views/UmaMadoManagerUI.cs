using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using UmaMadoManager.Core.ViewModels;
using UmaMadoManager.Core.Extensions;
using UmaMadoManager.Core.Models;
using System.Reflection;

namespace UmaMadoManager.Windows.Views
{
    public class UmaMadoManagerUI
    {
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public UmaMadoManagerUI(AxisStandardViewModel viewModel)
        {
            var _VM = viewModel;
            var trayNotifyIcon = new NotifyIcon()
            {
                Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("UmaMadoManager.Windows.Resources.TrayIcon.ico")),
                Visible = true,
            };

            this.Disposable.Add(_VM.Vertical.CombineLatest(_VM.Horizontal).Subscribe((x) =>
            {
                trayNotifyIcon.Text = $"V => {x.First}: H => {x.Second}"; // もうちょいわかりやすく
            }));

            var contextMenu = new ContextMenuStrip();

            var verticalSettingMenus = new ToolStripMenuItem()
            {
                Text = "Vertical",
            };
            verticalSettingMenus.DropDownItems.AddRange(new ToolStripItem[]{
                new ToolStripMenuItem("Application default").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.Vertical.Value = AxisStandard.Application;
                    }));
                    this.Disposable.Add(_VM.Vertical.Subscribe(x => {
                        v.Checked = x == AxisStandard.Application;
                    }));
                    v.CheckOnClick = true;
                }),
                // new ToolStripMenuItem("User defined").Also(v => {
                //     this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                //         _VM.Vertical.Value = AxisStandard.User;
                //     }));
                //     this.Disposable.Add(_VM.Vertical.Subscribe(x => {
                //         v.Checked = x == AxisStandard.User;
                //     }));
                //     v.CheckOnClick = true;
                // }),
                new ToolStripMenuItem("Full height").Also(v => {
                    v.DropDownItems.AddRange(new ToolStripItem[]{
                        new ToolStripMenuItem("Left Top").Also(vv => {
                            this.Disposable.Add(Observable.FromEventPattern(vv, nameof(vv.Click)).Subscribe(x => {
                                _VM.Vertical.Value = AxisStandard.Full;
                                _VM.WindowFittingStandard.Value = WindowFittingStandard.LeftTop;
                            }));
                            this.Disposable.Add(_VM.Vertical.CombineLatest(_VM.WindowFittingStandard).Subscribe(x => {
                                vv.Checked = x == (AxisStandard.Full, WindowFittingStandard.LeftTop);
                            }));
                            vv.CheckOnClick = true;
                        }),
                        new ToolStripMenuItem("Right Top").Also(vv => {
                            this.Disposable.Add(Observable.FromEventPattern(vv, nameof(vv.Click)).Subscribe(x => {
                                _VM.Vertical.Value = AxisStandard.Full;
                                _VM.WindowFittingStandard.Value = WindowFittingStandard.RightTop;
                            }));
                            this.Disposable.Add(_VM.Vertical.CombineLatest(_VM.WindowFittingStandard).Subscribe(x => {
                                vv.Checked = x == (AxisStandard.Full, WindowFittingStandard.RightTop);
                            }));
                            vv.CheckOnClick = true;
                        })
                    });
                }),
            });

            var horizontalSettingMenus = new ToolStripMenuItem()
            {
                Text = "Horizontal",
            };
            horizontalSettingMenus.DropDownItems.AddRange(new ToolStripItem[]{
                new ToolStripMenuItem("Application default").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.Horizontal.Value = AxisStandard.Application;
                    }));
                    this.Disposable.Add(_VM.Horizontal.Subscribe(x => {
                        v.Checked = x == AxisStandard.Application;
                    }));
                    v.CheckOnClick = true;
                }),
                // new ToolStripMenuItem("User defined").Also(v => {
                //     this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                //         _VM.Horizontal.Value = AxisStandard.User;
                //     }));
                //     this.Disposable.Add(_VM.Horizontal.Subscribe(x => {
                //         v.Checked = x == AxisStandard.User;
                //     }));
                //     v.CheckOnClick = true;
                // }),
                new ToolStripMenuItem("Full width").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.Horizontal.Value = AxisStandard.Full;
                    }));
                    this.Disposable.Add(_VM.Horizontal.Subscribe(x => {
                        v.Checked = x == AxisStandard.Full;
                    }));
                    v.CheckOnClick = true;
                }),
            });

            var muteSettingMenus = new ToolStripMenuItem()
            {
                Text = "Mute",
            };
            muteSettingMenus.DropDownItems.AddRange(new ToolStripItem[]{
                new ToolStripMenuItem("Nop").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.MuteCondition.Value = MuteCondition.Nop;
                    }));
                    this.Disposable.Add(_VM.MuteCondition.Subscribe(x => {
                        v.Checked = x == MuteCondition.Nop;
                    }));
                    v.CheckOnClick = true;
                }),
                new ToolStripMenuItem("When Background").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.MuteCondition.Value = MuteCondition.WhenBackground;
                    }));
                    this.Disposable.Add(_VM.MuteCondition.Subscribe(x => {
                        v.Checked = x == MuteCondition.WhenBackground;
                    }));
                    v.CheckOnClick = true;
                }),
                new ToolStripMenuItem("When Minimized").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        _VM.MuteCondition.Value = MuteCondition.WhenMinimize;
                    }));
                    this.Disposable.Add(_VM.MuteCondition.Subscribe(x => {
                        v.Checked = x == MuteCondition.WhenMinimize;
                    }));
                    v.CheckOnClick = true;
                }),
            });

            contextMenu.Items.AddRange(new ToolStripItem[]{
                verticalSettingMenus,
                horizontalSettingMenus,
                new ToolStripSeparator(),
                muteSettingMenus,
                new ToolStripSeparator(),
                new ToolStripMenuItem("Help").Also(v => {
                    v.Click += (_, _) => {
                        var url = "https://yamachu.booth.pm/items/2811984";

                        try
                        {
                            // see: https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e);
                        }
                    };
                }),
                new ToolStripMenuItem("New Version Released").Also(v => {
                    Disposable.Add(_VM.LatestVersion.Subscribe(version => {
                        if (version == "") {
                            v.Visible = false;
                            return;
                        }
                        v.Visible = Assembly.GetExecutingAssembly().GetName().Version.ToString() != version;
                    }));
                }),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit").Also(v => {
                    v.Click += (_, _) => {
                        trayNotifyIcon.Icon = null;
                        trayNotifyIcon.Dispose();
                        this.Disposable.Dispose();
                        Application.Exit();
                    };
                })
            });

            trayNotifyIcon.ContextMenuStrip = contextMenu;
        }
    }
}
