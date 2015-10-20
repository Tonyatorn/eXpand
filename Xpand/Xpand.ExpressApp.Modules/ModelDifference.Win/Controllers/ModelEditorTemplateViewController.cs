﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates.ActionControls.Binding;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Win.Controls;
using DevExpress.ExpressApp.Win.SystemModule;
using DevExpress.ExpressApp.Win.Templates;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using Fasterflect;
using Xpand.ExpressApp.ModelDifference.DataStore.BaseObjects;
using Xpand.ExpressApp.ModelDifference.Win.PropertyEditors;

namespace Xpand.ExpressApp.ModelDifference.Win.Controllers {
    public class ModelEditorTemplateViewController : ViewController<ObjectView> {
        public ModelEditorTemplateViewController() {
            TargetObjectType = typeof(ModelDifferenceObject);
        }

        bool UseOldTemplates{
            get { return ((WinApplication) Application).UseOldTemplates; }
        }

        protected override void OnActivated(){
            base.OnActivated();
            if (!UseOldTemplates&& ((IModelOptionsWin) Application.Model.Options).FormStyle==RibbonFormStyle.Ribbon)
                Frame.GetController<ListViewProcessCurrentObjectController>().ProcessCurrentObjectAction.Execute+=ProcessCurrentObjectActionOnExecute;
        }

        protected override void OnDeactivated(){
            base.OnDeactivated();
            Frame.GetController<ListViewProcessCurrentObjectController>().ProcessCurrentObjectAction.Execute -= ProcessCurrentObjectActionOnExecute;
        }

        private void ProcessCurrentObjectActionOnExecute(object sender, SimpleActionExecuteEventArgs e){
            var modelEditorPropertyEditor = ((DetailView) e.ShowViewParameters.CreatedView).GetItems<ModelEditorPropertyEditor>().FirstOrDefault();
            if (modelEditorPropertyEditor != null){
                var controller = modelEditorPropertyEditor.ModelEditorViewModelEditorViewController;
                var mainBarActions = (Dictionary<ActionBase, string>) controller.GetFieldValue("mainBarActions");
                var actionBases = mainBarActions.Select(pair => pair.Key).ToArray().Where(@base => @base.Id!="Save").ToArray();
                foreach (var actionBase in actionBases){
                    actionBase.Category = "Unspecified";
                }
                e.ShowViewParameters.Controllers.Add(new ModelEditorActionsController(actionBases));
            }
        }

        class ModelEditorActionsController:ViewController<DetailView>{
            public ModelEditorActionsController(ActionBase[] actionBases){
                RegisterActions(actionBases);
            }
        }
        protected override void OnFrameAssigned(){
            base.OnFrameAssigned();
            if (!UseOldTemplates && ((IModelOptionsWin)Application.Model.Options).FormStyle == RibbonFormStyle.Ribbon) {
                var actionControlsSiteController = Frame.GetController<ActionControlsSiteController>();
                actionControlsSiteController.CustomBindActionControlToAction+=OnCustomBindActionControlToAction;
            }
        }

        private void OnCustomBindActionControlToAction(object sender, CustomBindEventArgs e){
            if (ModelEditorActions.Contains(e.Action)) {
                e.Binding = ActionBindingFactory.Instance.Create(e.Action, e.ActionControl);
            }
        }

        IEnumerable<ActionBase> ModelEditorActions{
            get{
                var controller = Frame.GetController<ModelEditorActionsController>();
                return controller != null ? controller.Actions.AsEnumerable() : Enumerable.Empty<ActionBase>();
            }
        }

        protected override void OnViewControlsCreated() {
            base.OnViewControlsCreated();
            var template = Frame.Template as XtraFormTemplateBase;
            if (template != null) {
                SetTemplate();
            }
            else{
                var editorPropertyEditor = View.GetItems<ModelEditorPropertyEditor>().FirstOrDefault();
                if (editorPropertyEditor != null){
                    var controller = editorPropertyEditor.ModelEditorViewModelEditorViewController;
                    controller.SetControl(editorPropertyEditor.Control);
                    controller.LoadSettings();
                }
            }
        }

        private void SetTemplate() {
            var editorPropertyEditor = View.GetItems<ModelEditorPropertyEditor>().FirstOrDefault();
            if (editorPropertyEditor!=null) {
                var controller = editorPropertyEditor.ModelEditorViewModelEditorViewController;
                var caption = Guid.NewGuid().ToString();
                controller.SaveAction.Caption = caption;
                controller.SetTemplate(Frame.Template);
                var barManagerHolder = ((IBarManagerHolder)Frame.Template);
                var barButtonItems = barManagerHolder.BarManager.Items.OfType<BarButtonItem>().ToArray();
                barButtonItems.First(item => item.Caption.IndexOf(caption, StringComparison.Ordinal) > -1).Visibility = BarItemVisibility.Never;
            }
        }
    }
}
