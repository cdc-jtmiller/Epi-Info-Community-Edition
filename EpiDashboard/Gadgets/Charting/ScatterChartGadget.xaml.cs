﻿using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for ScatterChartGadget.xaml
    /// </summary>
    public partial class ScatterChartGadget : GadgetBase
    {
        #region Constructors
            public ScatterChartGadget()
            {
                InitializeComponent();
                Construct();
            }

            public ScatterChartGadget(DashboardHelper dashboardHelper)
            {
                InitializeComponent();
                this.DashboardHelper = dashboardHelper;
                Construct();
            }

            private class XYChartData
            {
                public object X { get; set; }
                public double? Y { get; set; }
            }

            private class RegressionChartData
            {
                public object X { get; set; }
                public double Z { get; set; }
            }

            public class NumericDataValue
            {
                public decimal DependentValue { get; set; }
                public decimal IndependentValue { get; set; }
            }
        #endregion //Constructors
        
        #region Private and Protected Methods

            private void SetVisuals()
            {
                ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;
                //SetPalette();
                switch (chtParameters.Palette)
                {
                    case 0:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Atlantic");
                        break;
                    case 1:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Breeze");
                        break;
                    case 2:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("ComponentArt");
                        break;
                    case 3:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Deep");
                        break;
                    case 4:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Earth");
                        break;
                    case 5:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Evergreen");
                        break;
                    case 6:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Heatwave");
                        break;
                    case 7:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Montreal");
                        break;
                    case 8:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Pastel");
                        break;
                    case 9:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Renaissance");
                        break;
                    case 10:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("SharePoint");
                        break;
                    case 11:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Study");
                        break;
                    default:
                    case 12:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantA");
                        break;
                    case 13:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantB");
                        break;
                    case 14:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantC");
                        break;
                }

                //SetMarkerType();
                xyChart.Legend.FontSize = chtParameters.LegendFontSize;

                xAxisCoordinates.Angle = chtParameters.XAxisAngle;
               

                //--Ei-196
                 //tblockYAxisLabel.Text = chtParameters.YAxisLabel;
                 tblockYAxisLabel.Text = chtParameters.CrosstabVariableName;
                //

                switch (chtParameters.XAxisLabelType)
                {
                    case 3:
                        tblockXAxisLabel.Text = chtParameters.XAxisLabel;
                        break;
                    case 1:
                        if (chtParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(chtParameters.ColumnNames[0]))
                        {
                            Field field = DashboardHelper.GetAssociatedField(chtParameters.ColumnNames[0]);
                            if (field != null)
                            {
                                RenderableField rField = field as RenderableField;
                                tblockXAxisLabel.Text = rField.PromptText;
                            }
                            else
                            {
                                tblockXAxisLabel.Text = chtParameters.ColumnNames[0];
                             }
                        }
                        break;
                    case 2:
                        tblockXAxisLabel.Text = string.Empty;
                        break;
                    default:
                        tblockXAxisLabel.Text = chtParameters.ColumnNames[0];
                        break;
                }

                tblockChartTitle.Text = chtParameters.ChartTitle;
            }

            /// <summary>
            /// A custom heading to use for this gadget's output
            /// </summary>
            private string customOutputHeading;

            /// <summary>
            /// A custom description to use for this gadget's output
            /// </summary>
            private string customOutputDescription;

            protected override void Construct()
            {
                this.Parameters = new ScatterChartParameters();

                if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
                {
                    headerPanel.Text = CustomOutputHeading;
                }

                StrataGridList = new List<Grid>();
                StrataExpanderList = new List<Expander>();

                //mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
                mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

                //#if LINUX_BUILD
                //            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
                //#else
                //            mnuSendDataToExcel.Visibility = Visibility.Visible;
                //            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
                //            excelKey = key.OpenSubKey("Excel.Application");
                //            excelInstalled = excelKey == null ? false : true;

                //            if (!excelInstalled)
                //            {
                //                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
                //            }
                //            else
                //            {
                //                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
                //            }
                //#endif

                mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
                mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
                this.IsProcessing = false;
                this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
                this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

                base.Construct();
            }

            private void ClearResults()
            {
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;
                messagePanel.Text = string.Empty;
                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;

                xyChart.DataSource = null;
                xyChart.Width = 0;
                xyChart.Height = 0;

                StrataGridList.Clear();
                StrataExpanderList.Clear();
            }

            private void ShowLabels()
            {
                if (string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }

                if (string.IsNullOrEmpty(tblockYAxisLabel.Text.Trim()))
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }

                if (string.IsNullOrEmpty(tblockChartTitle.Text.Trim()))
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
                }
            }

            protected override void RenderFinish()
            {
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;

                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                messagePanel.Text = string.Empty;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;

                panelFormula.Visibility = System.Windows.Visibility.Visible;

                ShowLabels();
                HideConfigPanel();
                CheckAndSetPosition();
            }

            protected override void RenderFinishWithWarning(string errorMessage)
            {
                waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
                messagePanel.Text = errorMessage;
                messagePanel.Visibility = System.Windows.Visibility.Visible;

                panelFormula.Visibility = System.Windows.Visibility.Visible;

                ShowLabels();
                HideConfigPanel();
                CheckAndSetPosition();
            }

            protected override void RenderFinishWithError(string errorMessage)
            {
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;

                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
                messagePanel.Text = errorMessage;
                messagePanel.Visibility = System.Windows.Visibility.Visible;

                panelFormula.Visibility = System.Windows.Visibility.Collapsed;

                HideConfigPanel();
                CheckAndSetPosition();
            }

            private void SetChartData(List<XYChartData> dataList, StatisticsRepository.LinearRegression.LinearRegressionResults regresResults, NumericDataValue maxValue, NumericDataValue minValue)
            {
                List<RegressionChartData> regressionDataList = new List<RegressionChartData>();
                ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;

                if (regresResults.variables != null)
                {
                    decimal coefficient = Convert.ToDecimal(regresResults.variables[0].coefficient);
                    decimal constant = Convert.ToDecimal(regresResults.variables[1].coefficient);

                    NumericDataValue newMaxValue = new NumericDataValue();
                    newMaxValue.IndependentValue = maxValue.IndependentValue;
                    newMaxValue.DependentValue = (coefficient * maxValue.IndependentValue) + constant;
                    NumericDataValue newMinValue = new NumericDataValue();
                    newMinValue.IndependentValue = minValue.IndependentValue;
                    newMinValue.DependentValue = (coefficient * minValue.IndependentValue) + constant;

                    tblockEquation.Text = "Y = (" + Math.Round(coefficient, 4).ToString() + ")X + " + Math.Round(constant, 4).ToString();
                    
                    List<NumericDataValue> regresValues = new List<NumericDataValue>();
                    regresValues.Add(newMinValue);
                    regresValues.Add(newMaxValue);

                    RegressionChartData rChartData = new RegressionChartData();
                    rChartData.X = (double)newMinValue.IndependentValue;
                    rChartData.Z = (double)newMinValue.DependentValue;
                    regressionDataList.Add(rChartData);

                    rChartData = new RegressionChartData();
                    rChartData.X = (double)newMaxValue.IndependentValue;
                    rChartData.Z = (double)newMaxValue.DependentValue;
                    regressionDataList.Add(rChartData);

                    //---Ei-196
                    radiovars1.Content = chtParameters.ColumnNames[0].ToUpper() +" X " + chtParameters.CrosstabVariableName.ToUpper();
                    radiovars2.Content = "Linear Regression";
                    int value = (int)newMinValue.IndependentValue;
                    int remvalue = (value % 2);
                    xyChart.XRangeStart = newMinValue.IndependentValue - remvalue;
                    xyChart.XRangeEnd = newMaxValue.IndependentValue;
                               
                      NumericCoordinates numx = new NumericCoordinates();
                      numx.From =(double) newMinValue.IndependentValue ;
                      numx.To = (double) newMaxValue.IndependentValue; 
                      numx.Step = 4;
                      AxisCoordinates ax = new AxisCoordinates();
                      ax.Coordinates = numx;
                      ax.Visibility = System.Windows.Visibility.Visible;
                      xyChart.XAxisArea.Clear();
                      xyChart.XAxisArea.Add(ax);
                   //--
                    
                }

                //xAxis.UseOnlyVisiblePointsToComputeRange = true;

                
                series0.DataSource = dataList;
                series1.DataSource = regressionDataList;
                xyChart.Width =  chtParameters.ChartWidth;
                xyChart.Height = chtParameters.ChartHeight;
               


                //xAxis.UseOnlyVisiblePointsToComputeRange = true;

            }

            private delegate void SetChartDataDelegate(List<XYChartData> dataList, StatisticsRepository.LinearRegression.LinearRegressionResults regresResults, NumericDataValue maxValue, NumericDataValue minValue);

        #endregion //Private and Protected Methods

        #region Public Methods
            public override void SetGadgetToProcessingState()
            {
                this.IsProcessing = true;
            }

            public override void SetGadgetToFinishedState()
            {
                this.IsProcessing = false;
                base.SetGadgetToFinishedState();
            }

            public override void RefreshResults()
            {
                ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;
                if (!LoadingCombos)
                {
                    if (chtParameters != null)
                    {
                        if (chtParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(chtParameters.ColumnNames[0]))
                        {
                            SetVisuals();
                            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                            waitPanel.Visibility = System.Windows.Visibility.Visible;
                            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                            baseWorker = new BackgroundWorker();
                            baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                            baseWorker.RunWorkerAsync();
                            base.RefreshResults();
                        }
                        else
                        {
                            ClearResults();
                            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                }
            }

            /// <summary>
            /// Converts the gadget's output to Html
            /// </summary>
            /// <returns></returns>
            public override string ToHTML(string htmlFileName = "", int count = 0)
            {
                // Check to see if a chart has been created.
                //if (xyChart.ActualHeight == 0 || xyChart.ActualWidth == 0)
                //{
                //    return string.Empty;
                //}

                StringBuilder htmlBuilder = new StringBuilder();

                if (string.IsNullOrEmpty(CustomOutputHeading))
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Chart</h2>");
                }
                else if (CustomOutputHeading != "(none)")
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
                }

                if (!string.IsNullOrEmpty(CustomOutputDescription))
                {
                    htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
                }

                if (!string.IsNullOrEmpty(tblockEquation.Text))
                {
                    htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
                    htmlBuilder.AppendLine("<em>Equation:</em> <strong>" + tblockEquation.Text + "</strong>");
                    htmlBuilder.AppendLine("<br />");
                }

                string imageFileName = string.Empty;

                if (htmlFileName.EndsWith(".html"))
                {
                    imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
                }
                else if (htmlFileName.EndsWith(".htm"))
                {
                    imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
                }

                imageFileName = imageFileName + "_" + count.ToString() + ".png";

                //BitmapSource img = (BitmapSource)Common.ToImageSource(xyChart);
                System.IO.FileStream stream = new System.IO.FileStream(imageFileName, System.IO.FileMode.Create);
                //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();

                htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" />");

                return htmlBuilder.ToString();
            }

            public override void ShowHideConfigPanel()
            {
                Popup = new DashboardPopup();
                Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
                Controls.GadgetProperties.ScatterChartProperties properties = new Controls.GadgetProperties.ScatterChartProperties(this.DashboardHelper, this, (ScatterChartParameters)Parameters, StrataGridList);

                properties.Width = 800;
                properties.Height = 600;

                if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > properties.Width)
                {
                    properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
                }

                if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > properties.Height)
                {
                    properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
                }

                properties.Cancelled += new EventHandler(properties_Cancelled);
                properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
                Popup.Content = properties;
                Popup.Show();
            }

            public override void CreateFromXml(XmlElement element)
            {
                this.LoadingCombos = true;
                this.Parameters = new ScatterChartParameters();

                HideConfigPanel();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;

                foreach (XmlElement child in element.ChildNodes)
                {
                    switch (child.Name.ToLower())
                    {
                        case "mainvariable":
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((ScatterChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((ScatterChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "outcomevariable":
                            ((ScatterChartParameters)Parameters).CrosstabVariableName = (child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "customheading":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                            }
                            else
                            {
                                this.CustomOutputHeading = string.Empty;
                                this.Parameters.GadgetTitle = string.Empty;
                            }
                            break;
                        case "customdescription":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
                                if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                                {
                                    descriptionPanel.Text = CustomOutputDescription;
                                    descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                                }
                                else
                                {
                                    descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                                }
                            }
                            break;
                        case "customcaption":
                            this.CustomOutputCaption = child.InnerText;
                            break;
                        case "datafilters":
                            this.DataFilters = new DataFilters(this.DashboardHelper);
                            this.DataFilters.CreateFromXml(child);
                            break;
                        case "palette":
                            ((ScatterChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                            break;
                        case "areatype":
                            ((ScatterChartParameters)Parameters).MarkerType = int.Parse(child.InnerText);
                            break;
                        case "yaxislabel":
                            ((ScatterChartParameters)Parameters).YAxisLabel = child.InnerText;
                            break;
                        case "xaxislabeltype":
                            ((ScatterChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                            break;
                        case "xaxislabel":
                            ((ScatterChartParameters)Parameters).XAxisLabel = child.InnerText;
                            break;
                        case "xaxisangle":
                            ((ScatterChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                            break;
                        case "charttitle":
                            ((ScatterChartParameters)Parameters).ChartTitle = child.InnerText;
                            break;
                        case "chartsubtitle":
                            ((ScatterChartParameters)Parameters).ChartSubTitle = child.InnerText;
                            break;
                        case "showlegend":
                            if (child.InnerText.ToLower().Equals("true")) { ((ScatterChartParameters)Parameters).ShowLegend = true; }
                            else { ((ScatterChartParameters)Parameters).ShowLegend = false; }
                            break;
                        case "showlegendborder":
                            if (child.InnerText.ToLower().Equals("true")) { ((ScatterChartParameters)Parameters).ShowLegendBorder = true; }
                            else { ((ScatterChartParameters)Parameters).ShowLegendBorder = false; }
                            break;
                        case "showlegendvarnames":
                            if (child.InnerText.ToLower().Equals("true")) { ((ScatterChartParameters)Parameters).ShowLegendVarNames = true; }
                            else { ((ScatterChartParameters)Parameters).ShowLegendVarNames = false; }
                            break;
                        case "legendfontsize":
                            ((ScatterChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                            break;
                        case "height":
                            ((ScatterChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                            break;
                        case "width":
                            ((ScatterChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
                            break;
                    }
                }

                base.CreateFromXml(element);

                this.LoadingCombos = false;
                if (!String.IsNullOrEmpty(((ScatterChartParameters)Parameters).ColumnNames[0]) && (!String.IsNullOrEmpty(((ScatterChartParameters)Parameters).CrosstabVariableName)))
                {
                    RefreshResults();
                }
                HideConfigPanel();
            }

            /// <summary>
            /// Serializes the gadget into Xml
            /// </summary>
            /// <param name="doc">The Xml docment</param>
            /// <returns>XmlNode</returns>
            public override XmlNode Serialize(XmlDocument doc)
            {
                ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;

                System.Xml.XmlElement element = doc.CreateElement("scatterChartGadget");
                string xmlString = string.Empty;
                element.InnerXml = xmlString;
                element.AppendChild(SerializeFilters(doc));

                System.Xml.XmlAttribute id = doc.CreateAttribute("id");
                System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
                System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
                System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
                System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

                id.Value = this.UniqueIdentifier.ToString();
                locationY.Value = Canvas.GetTop(this).ToString("F0");
                locationX.Value = Canvas.GetLeft(this).ToString("F0");
                collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
                type.Value = "EpiDashboard.Gadgets.Charting.ScatterChartGadget";

                element.Attributes.Append(locationY);
                element.Attributes.Append(locationX);
                element.Attributes.Append(collapsed);
                element.Attributes.Append(type);
                element.Attributes.Append(id);


                string mainVar = string.Empty;
                string crosstabVar = string.Empty;
                string strataVar = string.Empty;
                string weightVar = string.Empty;
                string sort = string.Empty;

                double height = 600;
                double width = 800;

                double.TryParse(chtParameters.ChartHeight.ToString(), out height);
                double.TryParse(chtParameters.ChartWidth.ToString(), out width);

                //mainVariable
                XmlElement freqVarElement = doc.CreateElement("mainVariable");
                if (chtParameters.ColumnNames.Count > 0)
                {
                    if (!String.IsNullOrEmpty(chtParameters.ColumnNames[0].ToString()))
                    {
                        freqVarElement.InnerText = chtParameters.ColumnNames[0].ToString().Replace("<", "&lt;");
                        element.AppendChild(freqVarElement);
                    }
                }

                //Outcome
                XmlElement outcomeVarElement = doc.CreateElement("outcomeVariable");
                outcomeVarElement.InnerText = chtParameters.CrosstabVariableName.ToString().Replace("<", "&lt;");
                element.AppendChild(outcomeVarElement);

                CustomOutputHeading = headerPanel.Text;
                CustomOutputDescription = descriptionPanel.Text;

                //height 
                XmlElement heightElement = doc.CreateElement("height");
                heightElement.InnerText = chtParameters.ChartHeight.ToString().Replace("<", "&lt;");
                element.AppendChild(heightElement);

                //width 
                XmlElement widthElement = doc.CreateElement("width");
                widthElement.InnerText = chtParameters.ChartWidth.ToString().Replace("<", "&lt;");
                element.AppendChild(widthElement);

                //customHeading
                XmlElement customHeadingElement = doc.CreateElement("customHeading");
                customHeadingElement.InnerText = chtParameters.GadgetTitle.Replace("<", "&lt;");
                element.AppendChild(customHeadingElement);

                //customDescription
                XmlElement customDescriptionElement = doc.CreateElement("customDescription");
                customDescriptionElement.InnerText = chtParameters.GadgetDescription.Replace("<", "&lt;");
                element.AppendChild(customDescriptionElement);

                //customCaption
                XmlElement customCaptionElement = doc.CreateElement("customCaption");
                if (!String.IsNullOrEmpty(CustomOutputCaption))
                {
                    customCaptionElement.InnerText = CustomOutputCaption.Replace("<", "&lt;");
                }
                else
                {
                    customCaptionElement.InnerText = string.Empty;
                }
                element.AppendChild(customCaptionElement);

                //palette 
                XmlElement paletteElement = doc.CreateElement("palette");
                paletteElement.InnerText = chtParameters.Palette.ToString();
                element.AppendChild(paletteElement);

                //Marker type 
                XmlElement markerTypeElement = doc.CreateElement("areaType");
                markerTypeElement.InnerText = chtParameters.MarkerType.ToString();
                element.AppendChild(markerTypeElement);

                //yAxisLabel 
                XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
                yAxisLabelElement.InnerText = chtParameters.YAxisLabel.ToString().Replace("<", "&lt;");
                element.AppendChild(yAxisLabelElement);

                //xAxisLabelType 
                XmlElement xAxisLabelTypeElement = doc.CreateElement("xAxisLabelType");
                xAxisLabelTypeElement.InnerText = chtParameters.XAxisLabelType.ToString().Replace("<", "&lt;");
                element.AppendChild(xAxisLabelTypeElement);

                //xAxisLabel 
                XmlElement xAxisLabelElement = doc.CreateElement("xAxisLabel");
                xAxisLabelElement.InnerText = chtParameters.XAxisLabel.ToString().Replace("<", "&lt;");
                element.AppendChild(xAxisLabelElement);

                //xAxisAngle 
                XmlElement xAxisAngleElement = doc.CreateElement("xAxisAngle");
                xAxisAngleElement.InnerText = chtParameters.XAxisAngle.ToString().Replace("<", "&lt;");
                element.AppendChild(xAxisAngleElement);

                //chartTitle 
                XmlElement chartTitleElement = doc.CreateElement("chartTitle");
                chartTitleElement.InnerText = chtParameters.ChartTitle.ToString().Replace("<", "&lt;");
                element.AppendChild(chartTitleElement);

                //chartSubTitle 
                XmlElement chartSubTitleElement = doc.CreateElement("chartSubTitle");
                chartSubTitleElement.InnerText = chtParameters.ChartSubTitle.ToString().Replace("<", "&lt;");
                element.AppendChild(chartSubTitleElement);

                //showLegend 
                XmlElement showLegendElement = doc.CreateElement("showLegend");
                showLegendElement.InnerText = chtParameters.ShowLegend.ToString().Replace("<", "&lt;");
                element.AppendChild(showLegendElement);

                //showLegendBorder 
                XmlElement showLegendBorderElement = doc.CreateElement("showLegendBorder");
                showLegendBorderElement.InnerText = chtParameters.ShowLegendBorder.ToString();
                element.AppendChild(showLegendBorderElement);

                //showLegendVarNames 
                XmlElement showLegendVarNamesElement = doc.CreateElement("showLegendVarNames");
                showLegendVarNamesElement.InnerText = chtParameters.ShowLegendVarNames.ToString();
                element.AppendChild(showLegendVarNamesElement);

                //legendFontSize 
                XmlElement legendFontSizeElement = doc.CreateElement("legendFontSize");
                legendFontSizeElement.InnerText = chtParameters.LegendFontSize.ToString();
                element.AppendChild(legendFontSizeElement);

                //legendDock 
                XmlElement legendDockElement = doc.CreateElement("legendDock");
                switch (chtParameters.LegendDock)
                {
                    case ComponentArt.Win.DataVisualization.Charting.Dock.Left:
                        legendDockElement.InnerText = "0";
                        break;
                    default:
                    case ComponentArt.Win.DataVisualization.Charting.Dock.Right:
                        legendDockElement.InnerText = "1";
                        break;
                    case ComponentArt.Win.DataVisualization.Charting.Dock.Top:
                        legendDockElement.InnerText = "2";
                        break;
                    case ComponentArt.Win.DataVisualization.Charting.Dock.Bottom:
                        legendDockElement.InnerText = "3";
                        break;
                }
                element.AppendChild(legendDockElement);

                SerializeAnchors(element);

                return element;
            }

            public virtual void ToImageFile(string fileName, bool includeGrid = true)
            {
                BitmapSource img = ToBitmapSource(false);

                System.IO.FileStream stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
                BitmapEncoder encoder = null; // new BitmapEncoder();

                if (fileName.EndsWith(".png"))
                {
                    encoder = new PngBitmapEncoder();
                }
                else
                {
                    encoder = new JpegBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();
            }

            public virtual BitmapSource ToBitmapSource(bool includeGrid = true)
            {
                ComponentArt.Win.DataVisualization.Charting.ChartBase xyChart = null;
                object el = FindName("xyChart");
                if (el is XYChart)
                {
                    xyChart = el as XYChart;
                }
                else if (el is ComponentArt.Win.DataVisualization.Charting.PieChart)
                {
                    xyChart = el as ComponentArt.Win.DataVisualization.Charting.PieChart;
                }

                if (xyChart == null) throw new ApplicationException();

                Transform transform = xyChart.LayoutTransform;
                Thickness margin = new Thickness(xyChart.Margin.Left, xyChart.Margin.Top, xyChart.Margin.Right, xyChart.Margin.Bottom);
                // reset current transform (in case it is scaled or rotated)
                xyChart.LayoutTransform = null;
                xyChart.Margin = new Thickness(0);

                Size size = new Size(xyChart.Width, xyChart.Height);
                // Measure and arrange the surface
                // VERY IMPORTANT
                xyChart.Measure(size);
                xyChart.Arrange(new Rect(size));

                RenderTargetBitmap renderBitmap =
                  new RenderTargetBitmap(
                    (int)size.Width,
                    (int)size.Height,
                    96d,
                    96d,
                    PixelFormats.Pbgra32);
                renderBitmap.Render(xyChart);

                xyChart.LayoutTransform = transform;
                xyChart.Margin = margin;

                xyChart.Measure(size);
                xyChart.Arrange(new Rect(size));

                return renderBitmap;
            }

            public void SaveImageToFile()
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".png";
                dlg.Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg";

                if (dlg.ShowDialog().Value)
                {
                    ToImageFile(dlg.FileName);
                    MessageBox.Show("Image saved successfully.", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            public virtual void CopyImageToClipboard()
            {
                BitmapSource img = ToBitmapSource(false); // renderBitmap;
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                Clipboard.SetImage(encoder.Frames[0]);
            }

            public virtual void CopyAllDataToClipboard()
            {
                object el = FindName("xyChart");
                if (el is XYChart)
                {
                    Clipboard.Clear();
                    Clipboard.SetText(SendDataToString());
                }
            }

            public virtual string SendDataToString()
            {
                StringBuilder sb = new StringBuilder();

                return sb.ToString();
            }

        #endregion //Public Methods

        #region Event Handlers
            private void properties_ChangesAccepted(object sender, EventArgs e)
            {
                Controls.GadgetProperties.ScatterChartProperties properties = Popup.Content as Controls.GadgetProperties.ScatterChartProperties;
                this.Parameters = properties.Parameters;
                this.DataFilters = properties.DataFilters;
                this.CustomOutputHeading = Parameters.GadgetTitle;
                this.CustomOutputDescription = Parameters.GadgetDescription;
                Popup.Close();
                if (properties.HasSelectedFields)
                {
                    RefreshResults();
                }
            }

            private void properties_Cancelled(object sender, EventArgs e)
            {
                Popup.Close();
            }

            protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
            {
                if (DashboardHelper.IsAutoClosing)
                {
                    System.Threading.Thread.Sleep(2200);
                }
                else
                {
                    System.Threading.Thread.Sleep(300);
                }
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            }

            protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
            {
                lock (syncLock)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                    ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;

                    string freqVar = chtParameters.ColumnNames[0];
                    string crosstabVar = chtParameters.CrosstabVariableName;
                    bool includeMissing = chtParameters.IncludeMissing;

                    try
                    {
                        RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                        CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                        chtParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                        chtParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                        if (this.DataFilters != null && this.DataFilters.Count > 0)
                        {
                            chtParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                        }
                        else
                        {
                            chtParameters.CustomFilter = string.Empty;
                        }

                        DataView dv = DashboardHelper.GenerateView(chtParameters);

                        List<XYChartData> dataList = new List<XYChartData>();

                        NumericDataValue minValue = null;
                        NumericDataValue maxValue = null;

                        foreach (DataRowView drv in dv)
                        {
                            DataRow row = drv.Row;

                            if (row[freqVar] != DBNull.Value && row[freqVar] != null && row[crosstabVar] != DBNull.Value && row[crosstabVar] != null)
                            {
                                XYChartData chartData = new XYChartData();
                                chartData.X = Convert.ToDouble(row[freqVar]);
                                chartData.Y = Convert.ToDouble(row[crosstabVar]);
                                dataList.Add(chartData);

                                NumericDataValue currentValue = new NumericDataValue() { DependentValue = Convert.ToDecimal(row[crosstabVar]), IndependentValue = Convert.ToDecimal(row[freqVar]) };
                                if (minValue == null)
                                {
                                    minValue = currentValue;
                                }
                                else
                                {
                                    if (currentValue.IndependentValue < minValue.IndependentValue)
                                    {
                                        minValue = currentValue;
                                    }
                                }
                                if (maxValue == null)
                                {
                                    maxValue = currentValue;
                                }
                                else
                                {
                                    if (currentValue.IndependentValue > maxValue.IndependentValue)
                                    {
                                        maxValue = currentValue;
                                    }
                                }
                            }
                        }

                        StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();
                        Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                        inputVariableList.Add(crosstabVar, "dependvar");
                        inputVariableList.Add("intercept", "true");
                        inputVariableList.Add("includemissing", "false");
                        inputVariableList.Add("p", "0.95");
                        inputVariableList.Add(freqVar, "unsorted");

                        StatisticsRepository.LinearRegression.LinearRegressionResults regresResults = linearRegression.LinearRegression(inputVariableList, dv.Table);

                        this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, regresResults, maxValue, minValue);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));

                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    }
                    finally
                    {
                        stopwatch.Stop();
                        Debug.Print("Scatter chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                        Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                    }
                }
            }

            private void btnRun_Click(object sender, RoutedEventArgs e)
            {
                if (LoadingCombos)
                {
                    return;
                }

                RefreshResults();
            }

            private void xyChart_DataStructureCreated(object sender, EventArgs e)
            {
                ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;
                string sName = "";

                if (chtParameters.StrataVariableNames.Count > 0)
                {
                    foreach (Series s0 in xyChart.DataSeries)
                    {
                        sName = s0.Label.Split('.')[1];
                        if (chtParameters.ShowLegendVarNames == false)
                        {
                            int index = sName.IndexOf(" = ");
                            s0.Label = sName.Substring(index + 3);
                        }
                        else
                        {
                            s0.Label = sName;
                        }
                    }
                }
                              
            }

            private void mnuSaveChartAsImage_Click(object sender, RoutedEventArgs e)
            {
                SaveImageToFile();
            }

            private void mnuCopyChartImage_Click(object sender, RoutedEventArgs e)
            {
                CopyImageToClipboard();
            }

            private void mnuCopyChartData_Click(object sender, RoutedEventArgs e)
            {
                CopyAllDataToClipboard();
            }

        #endregion // Event Handlers

        #region Private Properties
            private bool IsDropDownList { get; set; }
            private bool IsCommentLegal { get; set; }
            private bool IsRecoded { get; set; }
            private bool IsOptionField { get; set; }

            /// <summary>
            /// Gets/sets the gadget's custom output heading
            /// </summary>
            public override string CustomOutputHeading
            {
                get
                {
                    return this.customOutputHeading;
                }
                set
                {
                    this.customOutputHeading = value;
                    headerPanel.Text = CustomOutputHeading;
                }
            }

            /// <summary>
            /// Gets/sets the gadget's custom output description
            /// </summary>
            public override string CustomOutputDescription
            {
                get
                {
                    return this.customOutputDescription;
                }
                set
                {
                    this.customOutputDescription = value;
                    descriptionPanel.Text = CustomOutputDescription;
                }
            }

        #endregion //Private Properties

    }
}
