namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    class DialogModel
    {
        public bool Initialize(IOption option)
        {
            System.Windows.Window wnd
                = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>().Reverse()
                .FirstOrDefault(_ => _.DataContext?.GetType().Name == "WorkflowHostViewModel");

            if (wnd?.DataContext == null)
            {
                return false;
            }

            this.commands = GetBackNextCommand(wnd);

            if (!this.commands.HasAllCommands || !this.commands.GoNextCommand.CanExecute(null))
            {
                return false;
            }

            var piCurrentWorkflowId = wnd.DataContext.GetType().GetProperty("CurrentWorkflowId");
            if (piCurrentWorkflowId == null)
            {
                return false;
            }

            string workflowId = piCurrentWorkflowId.GetValue(wnd.DataContext)?.ToString();
            if (workflowId != "VS.IDE.NewProject")
            {
                return false;
            }

            var npdview = ControlFinder.FindChildren<System.Windows.Controls.ContentControl>(wnd)
                        .FirstOrDefault(_ => _.Name == "NPDView");

            if (npdview?.Content == null)
            {
                return false;
            }

            if (!this.page1.Initialize(option, npdview))
            {
                return false;
            }

            var grid = npdview.Parent as System.Windows.Controls.Grid;
            if (grid == null)
            {
                return false;
            }

            if (!this.page2.Initialize(option, npdview))
            {
                return false;
            }

            this.customView = new View.CustomProjectCreationView()
            {
                DataContext = this,
                Margin = new Thickness(5)
            };

#if DEBUG_
            System.Windows.Window w = new Window();
            w.Width = 500;
            w.Height = 300;
            w.Content = this.customView;
            w.Owner = System.Windows.Window.GetWindow(grid);
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Show();
#else
            
            npdview.Visibility = Visibility.Hidden;

            var wfcc = ControlFinder.FindChildren<ContentControl>(wnd).FirstOrDefault(_ => _.Name == "WorkflowContentControl");
            if (wfcc != null)
            {
                if (wfcc.Parent is Grid)
                {
                    Grid parent = (Grid)wfcc.Parent;
                    parent.RowDefinitions.Last().Height = GridLength.Auto;

                    if (Grid.GetColumn(wfcc) > 0)
                    {
                        Grid.SetColumn(wfcc, 0);
                        Grid.SetColumnSpan(wfcc, parent.ColumnDefinitions.Count);
                    }
                }
            }

            grid.Children.Add(this.customView);

            if (wfcc.Parent is Grid g)
            {
                TextBlock tb = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Left, FontSize = 20, FontWeight = FontWeights.Bold , Margin= new Thickness(10,2,0,2)};
                tb.SetBinding(TextBlock.TextProperty, "Title");
                tb.DataContext = page1.OriginalViewModel;
                Grid.SetRow(tb, Math.Max(0, Grid.GetRow(wfcc) - 1));
                Grid.SetColumnSpan(tb, 3);
                g.Children.Add(tb);
            }
    
#endif

            this.page1.SelectedTemplateChanged += CustomProjectTemplatesModel_SelectedItemChanged;

            wnd.ResizeMode = ResizeMode.CanResizeWithGrip;
            wnd.Closing += (s, e) =>
            {
                this.page1.WriteToOption(option);
                this.page2.WriteToOption(option);
                option.SaveSettingsToStorage();

                closed = true;
            };

            return true;
        }

        public CustomProjectTemplatesModel page1 { get; } = new CustomProjectTemplatesModel();
        public CustomProjectConfigurationModel page2 { get; private set; } = new CustomProjectConfigurationModel();

        private View.CustomProjectCreationView customView;
        private GoCommands commands;
        private bool closed;

        private static GoCommands GetBackNextCommand(Window wnd)
        {
            GoCommands cmds = new GoCommands();

            var d = wnd?.DataContext;
            if (d != null)
            {
                cmds.GoBackCommand = d.GetType().GetProperty("GoBack")?.GetValue(d) as ICommand;
                cmds.GoNextCommand = d.GetType().GetProperty("GoNext")?.GetValue(d) as ICommand;
            }
            return cmds;
        }

        private void CustomProjectTemplatesModel_SelectedItemChanged(object sender, EventArgs e)
        {
            if (!isSelectedItemChanged_delay)
            {
                isSelectedItemChanged_delay = true;
#pragma warning disable VSTHRD001
                GLOBAL.Dispatcher.BeginInvoke((Action)SelectedItemChangedDelay, System.Windows.Threading.DispatcherPriority.Loaded);
#pragma warning restore
            }
        }
        private bool isSelectedItemChanged_delay = false;

        private async void SelectedItemChangedDelay()
        {
            //コマンドのExecuteしてもViewへの反応が遅れるので捕まえるのがめんどくさい
            try
            {
                var ext = this.page1.SelectedExtension;
                if (ext == null)
                {
                    this.page2.OriginalViewModel = null;

                    return;
                }

                if (this.page2.IsSelected)
                {
                    //2ページ目にある場合は1ページ目に戻す必要が
                    for (DateTime t = DateTime.Now.AddSeconds(5); !commands.GoBackCommand.CanExecute(null);)
                    {
                        if (t <= DateTime.Now)
                        {
                            OnStateFail();
                            return;
                        }
                        await System.Threading.Tasks.Task.Delay(100);
                    }

                    commands.GoBackCommand.Execute(null);

                    if (!this.page1.IsSelected)
                    {
                        OnStateFail();
                        return;
                    }
                }


                if (this.page1.IsSelected)
                {//1ページ目にある場合は選択テンプレートをVisualStudio側に設定する

                    for (DateTime t = DateTime.Now.AddSeconds(5); ;)
                    {
                        ext = this.page1.SelectedExtension;//awaitの間にへんかしてるかも

                        this.page1.CopySelectedExtensionToToOriginal();
                        while (!commands.GoNextCommand.CanExecute(null))
                        {
                            if (t <= DateTime.Now)
                            {
                                OnStateFail();
                                return;
                            }
                            await System.Threading.Tasks.Task.Delay(100);
                        }

                        if (ext == this.page1.SelectedExtension)
                        {
                            break;
                        }
                    }

                    if (this.page1.IsSelected)
                    {
                        commands.GoNextCommand.Execute(null);
                    }
                }
            }
            catch(Exception ex)
            {
            }
            finally
            {
                isSelectedItemChanged_delay = false;
            }
        }

        private void OnStateFail()
        {
            if (this.customView != null)
            {

                if (this.customView?.Parent is Grid)
                {
                    ((Grid)this.customView.Parent).Children.Remove(customView);
                }

                customView = null;
                this.page1.Dispose();
                this.page2.Dispose();
            }
            Common.ShowError("State Fail");

            return;
        }

        class GoCommands
        {
            public ICommand GoBackCommand { get; set; }
            public ICommand GoNextCommand { get; set; }

            public bool HasAllCommands
            {
                get { return GoBackCommand != null && GoNextCommand != null; }
            }
        }

    }


}
