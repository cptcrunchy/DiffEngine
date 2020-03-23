using System;
using System.Collections;
using System.Windows;
using DifferenceEngine;

namespace DiffCalculator
{
    /// <summary>
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        private class LineItem
        {
            public int Line { get; set; }
            public string Text { get; set; }
            public string Status { get; set; }

        }


        public ResultsWindow(DiffListTextFile source, DiffListTextFile destination, ArrayList diffLines, double seconds)
        {
            InitializeComponent();
            Results_Header.Content = string.Format("File Diffs Calculated in {0} seconds.", TimeSpan.FromSeconds(seconds) );
            int count = 1;
            int i;

            foreach(DiffResultSpan drs in diffLines)
            {
                switch(drs.Status)
                {
                    case DiffResultSpanStatus.DeleteSource:
                        for(i = 0; i < drs.Length; i++)
                        {
                          
                            SourceListView.Items.Add(new LineItem
                            {
                                Line = count,
                                Text = ((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line,
                                Status = "Remove"
                            }) ;
                           

                            DestinationListView.Items.Add(new LineItem { 
                                Line = count,
                                Text = " ",
                                Status = "Ignore"
                            });

                            count++;

                        }
                    break;
                    case DiffResultSpanStatus.NoChange:
                        for (i = 0; i < drs.Length; i++)
                        {

                            SourceListView.Items.Add(new LineItem { 
                                Line = count,
                                Text = ((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line,
                                Status = "NoChange"
                            });

                            DestinationListView.Items.Add(new LineItem { 
                                Line = count,
                                Text = ((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line,
                                Status = "NoChange"
                            });

                            count++;
                        }
                    break;
                    case DiffResultSpanStatus.AddDestination:
                        for (i = 0; i < drs.Length; i++)
                        {
                            SourceListView.Items.Add(new LineItem
                            {
                                Line = count,
                                Text = " ",
                                Status = "Ignore"
                            });

                            DestinationListView.Items.Add(new LineItem
                            {
                                Line = count,
                                Text = ((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line,
                                Status = "Include"
                                
                            });

                            count++;
                        }
                    break;
                    case DiffResultSpanStatus.Replace:
                        for (i = 0; i < drs.Length; i++)
                        {
                            SourceListView.Items.Add(new LineItem
                            {
                                Line = count,
                                Text = ((TextLine)source.GetByIndex(drs.SourceIndex + i)).Line,
                                Status = "Remove"
                            });

                            DestinationListView.Items.Add(new LineItem
                            {
                                Line = count,
                                Text = ((TextLine)destination.GetByIndex(drs.DestIndex + i)).Line,
                                Status = "Include"
                            });

                            count++;

                        }
                    break;
                }
            }

        }

      
    }
}
