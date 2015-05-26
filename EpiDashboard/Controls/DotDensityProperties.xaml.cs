﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Mapping;
namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DotDensityProperties.xaml
    /// </summary>
    public partial class DotDensityProperties : UserControl
    {      
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        private EpiDashboard.Mapping.DotDensityLayerProvider provider;
        public event EventHandler FilterRequested;
        public RowFilterControl RowFilterControl { get; protected set; }       
        private double selectionGridHeight;
        private double guidedButtonsGridHeight;
        public DataFilters DataFilters;
        private Configuration config;
        private bool includeUserDefinedVars = true;
        private EpiDashboard.Dialogs.FilterDialogMode mode = EpiDashboard.Dialogs.FilterDialogMode.RowFilterMode;
        private EpiDashboard.Mapping.DotDensityKmlLayerProvider KMLprovider;
        private EpiDashboard.Mapping.DotDensityServerLayerProvider Mapprovider;
        public EpiDashboard.Mapping.DotDensityLayerProperties layerprop;
        public EpiDashboard.Mapping.DotDensityServerLayerProperties serverlayerprop;
        public EpiDashboard.Mapping.DotDensityKmlLayerProperties kmllayerprop;
        public event EventHandler MapGenerated;
        private string shapeFilePath;
        private System.Xml.XmlElement currentElement;
        public DotDensityProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            this.mapControl = mapControl;
            this.myMap = myMap;
            ////this.DashboardHelper = dashboardHelper;

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    //txtProjectPath.Text = dashboardHelper.View.Project.FilePath;

            //    if (System.IO.File.Exists(txtProjectPath.Text))
            //    {
            //        cmbFormName.Items.Clear();
            //        Project project = new Project(txtProjectPath.Text);
            //        foreach (View view in project.Views)
            //        {
            //            cmbFormName.Items.Add(view.Name);
            //        }
            //    }

            //    //cmbFormName.Text = dashboardHelper.View.Name;
            //}
            //else
            //{
            //    //if (!string.IsNullOrEmpty(dashboardHelper.CustomQuery))
            //    //{
            //    //    SqlQuery = DashboardHelper.CustomQuery;
            //    //}
            //}

            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + " unfiltered rows";
            //tblockColumns.Text = dashboardHelper.DataSet.Tables[0].Columns.Count.ToString() + " columns";
            //tblockCacheDateTime.Text = "Data last cached at " + dashboardHelper.LastCacheTime.ToShortDateString() + " " + dashboardHelper.LastCacheTime.ToShortTimeString();
            //tblockCacheTimeElapsed.Text = "Took " + dashboardHelper.TimeToCache + " to locally cache data";

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            //lbxRelatedDataSources.Items.Clear();
            //if (dashboardHelper.ConnectionsForRelate.Count > 0)
            //{
            //    // Related Data
            //    foreach (RelatedConnection rConn in dashboardHelper.ConnectionsForRelate)
            //    {
            //        lbxRelatedDataSources.Items.Add(rConn.db.ConnectionString);
            //    }
            //}       
         
           // ResetLegend_Click(new object(), new RoutedEventArgs());           
        }

        
        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }

        public FileInfo ProjectFileInfo
        {
            get 
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }
        

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Visible;
           // panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
           // panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            //panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;                          
            panelFilters.Visibility = System.Windows.Visibility.Visible;           
        }

       
        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RenderMap();
            if(layerprop!=null)
            layerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            else if(serverlayerprop!=null)
                serverlayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            else if(kmllayerprop!=null)
                kmllayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, txtDotValue.Text, ((SolidColorBrush)rctHighColor.Fill));
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            dashboardHelper = mapControl.GetNewDashboardHelper();
            this.DashboardHelper = dashboardHelper;                       
            if (dashboardHelper != null)
            {
                txtProjectPath.Text = dashboardHelper.Database.DbName;
                FillComboBoxes();
                this.DataFilters = new DataFilters(dashboardHelper);
                RowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, DataFilters, true);
                RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; RowFilterControl.FillSelectionComboboxes();
                panelFilters.Children.Add(RowFilterControl);
                txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
            }
        }
       
        private void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                cmbDataKey.Items.Add(field);
            }
            foreach (string field in numericFields)
            {
                cmbValue.Items.Add(field);
            }
            cmbValue.Items.Insert(0, "{Record Count}");
        }

        private void cmbFormName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cmbFormName.SelectedIndex >= 0)
            //{
            //    cmbFormName.Text = cmbFormName.SelectedItem.ToString();
            //}
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //cmbFormName.Items.Clear();
            //if (System.IO.File.Exists(txtProjectPath.Text))
            //{
            //    Project project = new Project(txtProjectPath.Text);
            //    foreach (View view in project.Views)
            //    {
            //        cmbFormName.Items.Add(view.Name);
            //    }
            //}
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            provider = new Mapping.DotDensityLayerProvider(myMap);           
            object[] shapeFileProperties = provider.LoadShapeFile();
            ILayerProperties layerProperties = null;
            layerProperties = new DotDensityLayerProperties(myMap, dashboardHelper, this.mapControl);
            layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
            this.layerprop = (DotDensityLayerProperties)layerProperties;
            if (this.DashboardHelper != null)
                layerprop.SetdashboardHelper(DashboardHelper);
            this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
            if (shapeFileProperties != null)
            {
                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    IDictionary<string, object> shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear(); 
                        layerprop.cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            layerprop.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
           
        }

        private void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();

                //List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                //    (SolidColorBrush)rctColor1.Fill, 
                //    (SolidColorBrush)rctColor2.Fill, 
                //    (SolidColorBrush)rctColor3.Fill, 
                //    (SolidColorBrush)rctColor4.Fill, 
                //    (SolidColorBrush)rctColor5.Fill, 
                //    (SolidColorBrush)rctColor6.Fill };

                //int classCount;
                //if (int.TryParse(cmbClasses.Text, out classCount))
                //{
                //    classCount = 4;
                //}
                if(provider!=null)
                provider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));               
                else if(Mapprovider!=null)
                    Mapprovider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));               
                else
                    KMLprovider.SetShapeRangeValues(dashboardHelper, cmbShapeKey.SelectedItem.ToString(), cmbDataKey.SelectedItem.ToString(), cmbValue.SelectedItem.ToString(), ((SolidColorBrush)rctHighColor.Fill).Color, int.Parse(txtDotValue.Text));               

            }
        }

        //private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor1.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor2.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor3.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor4.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor5.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor6.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctColor7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctColor7.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        //private void rctLowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
        //    }
        //}

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        //private void ResetLegend_Click(object sender, RoutedEventArgs e)
        //{
        //    if (rctLowColor == null || rctHighColor == null)
        //    {
        //        return;
        //    }

        //    int stratCount;

        //    SolidColorBrush rampStart;
        //    SolidColorBrush rampEnd;

        //    if (int.TryParse(cmbClasses.Text, out stratCount) == false)
        //    {
        //        stratCount = 4;
        //    }

        //    rampStart = (SolidColorBrush)rctLowColor.Fill;
        //    rampEnd = (SolidColorBrush)rctHighColor.Fill;

        //    int rd = rampStart.Color.R - rampEnd.Color.R;
        //    int gd = rampStart.Color.G - rampEnd.Color.G;
        //    int bd = rampStart.Color.B - rampEnd.Color.B;

        //    byte ri = (byte)(rd / (stratCount - 1));
        //    byte gi = (byte)(gd / (stratCount - 1));
        //    byte bi = (byte)(bd / (stratCount - 1));

        //    rctColor1.Fill = rampStart;

        //    int i = 3;

        //    Color coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi), (byte)(rampStart.Color.B - bi));
        //    rctColor2.Fill = new SolidColorBrush(coo);

        //    coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 2), (byte)(rampStart.Color.G - gi * 2), (byte)(rampStart.Color.B - bi * 2));
        //    rctColor3.Visibility = System.Windows.Visibility.Visible;
        //    if (i++ > stratCount)
        //    {
        //        coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //        rctColor3.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    rctColor3.Fill = new SolidColorBrush(coo);

        //    coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 3), (byte)(rampStart.Color.G - gi * 3), (byte)(rampStart.Color.B - bi * 3));
        //    rctColor4.Visibility = System.Windows.Visibility.Visible;
        //    if (i++ > stratCount)
        //    {
        //        coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //        rctColor4.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    rctColor4.Fill = new SolidColorBrush(coo);

        //    coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 4), (byte)(rampStart.Color.G - gi * 4), (byte)(rampStart.Color.B - bi * 4));
        //    rctColor5.Visibility = System.Windows.Visibility.Visible;
        //    if (i++ > stratCount)
        //    {
        //        coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //        rctColor5.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    rctColor5.Fill = new SolidColorBrush(coo);

        //    coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 5), (byte)(rampStart.Color.G - gi * 5), (byte)(rampStart.Color.B - bi * 5));
        //    rctColor6.Visibility = System.Windows.Visibility.Visible;
        //    if (i++ > stratCount)
        //    {
        //        coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //        rctColor6.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    rctColor6.Fill = new SolidColorBrush(coo);

        //    coo = Color.FromArgb(240, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 6), (byte)(rampStart.Color.B - bi * 6));
        //    rctColor7.Visibility = System.Windows.Visibility.Visible;
        //    if (i++ > stratCount)
        //    {
        //        coo = Color.FromArgb(240, byte.MaxValue, byte.MaxValue, byte.MaxValue);
        //        rctColor7.Visibility = System.Windows.Visibility.Hidden;
        //    }
        //    rctColor7.Fill = new SolidColorBrush(coo);
        //}

        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            KMLprovider = new Mapping.DotDensityKmlLayerProvider(myMap);
            KMLprovider.FeatureLoaded += new FeatureLoadedHandler(KMLprovider_FeatureLoaded);
            KMLprovider.LoadKml();
            ILayerProperties layerProperties = null;
            layerProperties = new DotDensityKmlLayerProperties(myMap, dashboardHelper, this.mapControl);
            layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
            this.kmllayerprop = (DotDensityKmlLayerProperties)layerProperties;
            if( this.DashboardHelper!=null)
                kmllayerprop.SetdashboardHelper(DashboardHelper); 
            this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
        }

        private void btnMapFile_Click(object sender, RoutedEventArgs e)
        {
            Mapprovider = new Mapping.DotDensityServerLayerProvider(myMap);
            Mapprovider.FeatureLoaded += new FeatureLoadedHandler(Mapprovider_FeatureLoaded);
            Mapprovider.LoadShapeFile();
            ILayerProperties layerProperties = null;
            layerProperties = new DotDensityServerLayerProperties(myMap, dashboardHelper, this.mapControl);
            layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(this.mapControl.ILayerProperties_FilterRequested);
            this.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
            if (this.DashboardHelper != null)
                serverlayerprop.SetdashboardHelper(DashboardHelper); 
            this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
        }


        void Mapprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    serverlayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        serverlayerprop.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                       cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                       rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                RenderMap();
            }
        }


        void KMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    kmllayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        kmllayerprop.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        rctHighColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                RenderMap();
            }
        }
        /// <summary>
        /// Handles the selection changed event for the Boundaries combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>       

        private void cmbBoundaries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (panelShapeFile != null)
            {
                if (cmbBoundaries.SelectedIndex == 0)
                {
                    panelShapeFile.Visibility = System.Windows.Visibility.Visible;
                    panelMapServer.Visibility = System.Windows.Visibility.Collapsed;
                    panelKML.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (cmbBoundaries.SelectedIndex == 1)
                {
                    panelShapeFile.Visibility = System.Windows.Visibility.Collapsed;
                    panelMapServer.Visibility = System.Windows.Visibility.Visible;
                    panelKML.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    panelShapeFile.Visibility = System.Windows.Visibility.Collapsed;
                    panelMapServer.Visibility = System.Windows.Visibility.Collapsed;
                    panelKML.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
       
    }
}