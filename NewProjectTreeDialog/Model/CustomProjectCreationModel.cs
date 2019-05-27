namespace Gekka.VisualStudio.Extention.NewProjectTreeDialog.Model
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    class CustomProjectCreationModel
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

            if (!this.CustomProjectTemplatesModel.Initialize(option, npdview))
            {
                return false;
            }

            var grid = npdview.Parent as System.Windows.Controls.Grid;
            if (grid == null)
            {
                return false;
            }
            var view = new View.CustomProjectCreationView() { DataContext = this };
            view.Margin = new Thickness(5);

            grid.Children.Add(view);

            if (!this.CustomProjectConfigurationModel.Initialize(option, npdview))
            {
                grid.Children.Remove(view);
                return false;
            }

            this.customView = view;
            this.CustomProjectTemplatesModel.SelectedTemplateChanged += CustomProjectTemplatesModel_SelectedItemChanged;


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


            wnd.ResizeMode = ResizeMode.CanResizeWithGrip;
            npdview.Visibility = Visibility.Hidden;

            wnd.Closing += (s, e) =>
              {
                  this.CustomProjectTemplatesModel.WriteToOption(option);
                  this.CustomProjectConfigurationModel.WriteToOption(option);
                  option.SaveSettingsToStorage();
              };

            return true;
        }



        public CustomProjectTemplatesModel CustomProjectTemplatesModel { get; } = new CustomProjectTemplatesModel();
        public CustomProjectConfigurationModel CustomProjectConfigurationModel { get; private set; } = new CustomProjectConfigurationModel();

        private View.CustomProjectCreationView customView;
        private GoCommands commands;

        private NPDViewDataContextType GetNPDViewDataContextType()
        {
            if (this.CustomProjectTemplatesModel.IsNpdviewSelected)
            {
                return NPDViewDataContextType.Creation;
            }
            if (this.CustomProjectConfigurationModel.IsNpdviewSelected)
            {
                return NPDViewDataContextType.Configuration;
            }
            return NPDViewDataContextType.Fail;
        }

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
            var state = GetNPDViewDataContextType();
            this.CustomProjectConfigurationModel.IsEnabled = false;

            if (state == NPDViewDataContextType.Configuration)
            {
                if (!commands.GoBackCommand.CanExecute(null))
                {
                    state = NPDViewDataContextType.Fail;
                }
                else
                {
                    commands.GoBackCommand.Execute(null);
                    state = GetNPDViewDataContextType();
                    if (state != NPDViewDataContextType.Creation)
                    {
                        state = NPDViewDataContextType.Fail;
                    }
                }
            }

            if (state == NPDViewDataContextType.Creation)
            {
                this.CustomProjectTemplatesModel.CopySelectedExtensionToToOriginal();
                if (this.CustomProjectTemplatesModel.SelectedExtension != null)
                {
                    if (!commands.GoNextCommand.CanExecute(null))
                    {
                        state = NPDViewDataContextType.Fail;
                        commands.GoNextCommand.CanExecute(null);
                    }
                    else
                    {
                        commands.GoNextCommand.Execute(null);
                        state = GetNPDViewDataContextType();
                        if (state == NPDViewDataContextType.Configuration)
                        {
                            this.CustomProjectConfigurationModel.UpdateTexts();
                            this.CustomProjectConfigurationModel.IsEnabled = true;
                        }
                        else
                        {
                            state = NPDViewDataContextType.Fail;
                        }
                    }
                }
            }

            if (state == NPDViewDataContextType.Fail)
            {
                if (this.customView != null)
                {
                    ((Grid)this.customView.Parent).Children.Remove(customView);
                    customView = null;
                    this.CustomProjectTemplatesModel.Dispose();
                    this.CustomProjectConfigurationModel.Dispose();
                }
                Common.ShowError("State Fail");

                return;
            }
        }


        enum NPDViewDataContextType
        {
            Fail,
            Creation,
            Configuration
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
