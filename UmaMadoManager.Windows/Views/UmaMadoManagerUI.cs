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

        private Form VerticalUserPositionSettingModal;
        private Form HorizontalUserPositionSettingModal;

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
                new ToolStripMenuItem("User defined").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        if (!VerticalUserPositionSettingModal.Visible)
                        {
                            // 動かせるように
                            _VM.Vertical.Value = AxisStandard.Application;
                            VerticalUserPositionSettingModal.Show();
                        }
                    }));
                    this.Disposable.Add(_VM.Vertical.Subscribe(x => {
                        v.Checked = x == AxisStandard.User;
                    }));
                    v.CheckOnClick = true;
                }),
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
                new ToolStripMenuItem("User defined").Also(v => {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x => {
                        if (!HorizontalUserPositionSettingModal.Visible)
                        {
                            _VM.Horizontal.Value = AxisStandard.Application;
                            HorizontalUserPositionSettingModal.Show();
                        }
                    }));
                    this.Disposable.Add(_VM.Horizontal.Subscribe(x => {
                        v.Checked = x == AxisStandard.User;
                    }));
                    v.CheckOnClick = true;
                }),
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

            Action navigateToHostingSite = () =>
            {
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

            contextMenu.Items.AddRange(new ToolStripItem[]{
                verticalSettingMenus,
                horizontalSettingMenus,
                new ToolStripSeparator(),
                muteSettingMenus,
                new ToolStripSeparator(),
                new ToolStripMenuItem("Extension").Also(v => {
                    v.DropDownItems.Add(new ToolStripMenuItem("AlwaysTop").Also(vv => {
                        this.Disposable.Add(Observable.FromEventPattern(vv, nameof(vv.Click)).Subscribe(x => {
                            _VM.IsMostTop.Value = !_VM.IsMostTop.Value;
                        }));
                        this.Disposable.Add(_VM.IsMostTop.Subscribe(x => {
                            vv.Checked = _VM.IsMostTop.Value;
                        }));
                        vv.CheckOnClick = true;
                    }));
                    v.DropDownItems.Add(new ToolStripMenuItem("RemoveBorder").Also(vv => {
                        this.Disposable.Add(Observable.FromEventPattern(vv, nameof(vv.Click)).Subscribe(x => {
                            _VM.IsRemoveBorder.Value = !_VM.IsRemoveBorder.Value;
                        }));
                        this.Disposable.Add(_VM.IsRemoveBorder.Subscribe(x => {
                            v.Checked = _VM.IsRemoveBorder.Value;
                        }));
                        v.CheckOnClick = true;
                    }));
                }),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Help").Also(v => {
                    v.Click += (_, _) => {
                        navigateToHostingSite();
                    };
                }),
                new ToolStripMenuItem("New Version Released").Also(v => {
                    Disposable.Add(_VM.LatestVersion.Subscribe(version => {
                        if (version == "") {
                            v.Visible = false;
                            return;
                        }
                        var currentVersionWithoutRevision = Assembly.GetExecutingAssembly().GetName().Version.Let(currentVersion => {
                            return new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
                        });
                        var formattedVersion = Version.Parse(version);
                        v.Visible = currentVersionWithoutRevision != formattedVersion;
                    }));
                    v.Click += (_, _) => {
                        navigateToHostingSite();
                    };
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

            Func<Action<Button>, Action<Button>?, Action<Button>, Form> createUserPositionSettingModal = (
                Action<Button> onClickUsePrevious,
                Action<Button> isUsePreviousEnable,
                Action<Button> onClickUseCurrent
            ) => new Form().Also(form =>
            {
                form.SuspendLayout();
                form.Size = new Size(300, 100);
                var layout = new TableLayoutPanel().Also(l =>
                {
                    l.SuspendLayout();
                    l.Dock = DockStyle.Fill;
                    l.RowCount = 2;
                    l.ColumnCount = 2;
                    l.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    l.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
                    new Label() { Text = "指定した位置とサイズを使用します" }.Also(c =>
                    {
                        c.AutoSize = true;
                        l.Controls.Add(c, 0, 0);
                        l.SetColumnSpan(c, 2);
                    });
                    l.Controls.Add(new Button().Also(b =>
                    {
                        b.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                        b.Text = "元の設定を使用";
                        onClickUsePrevious(b);
                        isUsePreviousEnable?.Invoke(b);
                    }), 0, 1);
                    l.Controls.Add(new Button().Also(b =>
                    {
                        b.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                        b.Text = "今の設定を使用";
                        onClickUseCurrent(b);
                    }), 1, 1);
                });
                form.Controls.Add(layout);
                layout.AutoSize = true;

                layout.ResumeLayout(false);
                layout.PerformLayout();

                form.ResumeLayout(false);
                form.MinimizeBox = false;
                form.MaximizeBox = false;
            });

            VerticalUserPositionSettingModal = createUserPositionSettingModal(
                (v) =>
                {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x =>
                    {
                        _VM.UseCurrentVerticalUserSetting.Value = false;
                        _VM.Vertical.Value = AxisStandard.User;
                        VerticalUserPositionSettingModal.Visible = false;
                    }));
                },
                (v) =>
                {
                    this.Disposable.Add(_VM.UserDefinedVerticalWindowRect.Subscribe(x =>
                    {
                        v.Enabled = !x.IsEmpty;
                    }));
                },
                (v) =>
                {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x =>
                    {
                        _VM.UseCurrentVerticalUserSetting.Value = true;
                        _VM.Vertical.Value = AxisStandard.User;
                        VerticalUserPositionSettingModal.Visible = false;
                    }));
                }
            );
            HorizontalUserPositionSettingModal = createUserPositionSettingModal(
                (v) =>
                {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x =>
                    {
                        _VM.UseCurrentHorizontalUserSetting.Value = false;
                        _VM.Horizontal.Value = AxisStandard.User;
                        HorizontalUserPositionSettingModal.Visible = false;
                    }));
                },
                (v) =>
                {
                    this.Disposable.Add(_VM.UserDefinedHorizontalWindowRect.Subscribe(x =>
                    {
                        v.Enabled = !x.IsEmpty;
                    }));
                },
                (v) =>
                {
                    this.Disposable.Add(Observable.FromEventPattern(v, nameof(v.Click)).Subscribe(x =>
                    {
                        _VM.UseCurrentHorizontalUserSetting.Value = true;
                        _VM.Horizontal.Value = AxisStandard.User;
                        HorizontalUserPositionSettingModal.Visible = false;
                    }));
                }
            );

            trayNotifyIcon.ContextMenuStrip = contextMenu;

            Application.ApplicationExit += (_, e) =>
            {
                _VM.OnExit();
            };
        }
    }
}
