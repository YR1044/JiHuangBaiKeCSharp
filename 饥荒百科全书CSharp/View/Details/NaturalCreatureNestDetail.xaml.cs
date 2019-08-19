﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 饥荒百科全书CSharp.Class;
using 饥荒百科全书CSharp.Class.JsonDeserialize;
using 饥荒百科全书CSharp.MyUserControl;

namespace 饥荒百科全书CSharp.View.Details
{
    /// <summary>
    /// NaturalCreatureNestDetail.xaml 的交互逻辑
    /// </summary>
    public partial class NaturalCreatureNestDetail : Page
    {
        private int _loadedTime;

        private string console;

        public void LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.ExtraData == null || _loadedTime != 0) return;
            _loadedTime++;
            LoadData((NatureCreatureNest)e.ExtraData);
            if (Global.FontFamily != null)
            {
                FontFamily = Global.FontFamily;
            }
            NaturalLeftScrollViewer.FontWeight = Global.FontWeight;
        }

        public NaturalCreatureNestDetail()
        {
            InitializeComponent();
            Global.NaturalLeftFrame.NavigationService.LoadCompleted += LoadCompleted;
            Global.ConsoleSendKey = Console_Click;
        }

        private readonly List<int?> _creatureNestHealthIntList = new List<int?>();
        private List<List<string>> _creatureNestCreatureListStringList;
        private List<List<string>> _creatureNestResourcesDestroyedListStringList;
        private List<List<string>> _creatureNestAbilityListStringList;
        private readonly List<string> _creatureNestConsoleStringList = new List<string>();
        private byte _creatureNestIndex;
        private int _creatureNestMaxIndex;
        private readonly List<string> _creatureNestList = new List<string>();

        private void LoadData(NatureCreatureNest c)
        {
            // 图片
            if (c.Pictures.Count == 0)
            {
                NatureImage.Source = new BitmapImage(new Uri(c.Picture, UriKind.Relative));
            }
            else
            {
                SwitchLeftButton.Visibility = Visibility.Visible;
                SwitchRightButton.Visibility = Visibility.Visible;
                // 数量分割点
                var breakPosition = c.Pictures[0].IndexOf('|');
                // 多名称多图
                if (breakPosition == -1)
                {
                    _creatureNestMaxIndex = c.Pictures.Count - 1;
                    foreach (var pic in c.Pictures)
                    {
                        _creatureNestList.Add(StringProcess.GetGameResourcePath(pic));
                    }
                    NatureImage.Source = new BitmapImage(new Uri(_creatureNestList[0], UriKind.Relative));
                    SwitchLeftButton.IsEnabled = false;
                }
                // 单名称多图
                else
                {
                    var pictureText = c.Pictures[0].Substring(0, breakPosition);
                    var pictureNum = int.Parse(c.Pictures[0].Substring(breakPosition + 1));
                    _creatureNestMaxIndex = pictureNum - 1;
                    for (var i = 1; i <= pictureNum; i++)
                    {
                        _creatureNestList.Add(StringProcess.GetGameResourcePath(pictureText + i));
                    }
                    NatureImage.Source = new BitmapImage(new Uri(_creatureNestList[0], UriKind.Relative));
                    SwitchLeftButton.IsEnabled = false;
                }
            }
            //中英文名
            NatureName.Text = c.Name;
            NatureEnName.Text = c.EnName;
            // 可再生、可摧毁
            if (c.IsRegenerate)
            {
                RegenerateCheckBox.IsChecked = true;
            }
            if (c.IsDestroable)
            {
                DestroableCheckBox.IsChecked = true;
            }
            else
            {
                RegenerateCheckBox.Visibility = Visibility.Collapsed;
                DestroableCheckBox.Margin = new Thickness(0, 0, 0, 0);
            }
            // 生命
            if (c.Health == null)
            {
                CreatureHealth.Visibility = Visibility.Collapsed;
            }
            else
            {
                CreatureHealth.Value = c.Health[0];
                CreatureHealth.BarColor = Global.ColorGreen;
                foreach (var health in c.Health)
                {
                    _creatureNestHealthIntList.Add(health);
                }
            }
            // 生物
            if (c.Creature.Count == 0)
            {
                NaturalCreatureTextBlock.Visibility = Visibility.Collapsed;
                NaturalCreatureStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                _creatureNestCreatureListStringList = new List<List<string>>();
                if (c.Creature.Count == 1)
                {
                    for (var i = 0; i <= _creatureNestMaxIndex; i++)
                        _creatureNestCreatureListStringList.Add(c.Creature[0]);
                }
                else
                {
                    foreach (var strList in c.Creature)
                    {
                        _creatureNestCreatureListStringList.Add(strList);
                    }
                }
                ShowCreature(0);
            }
            // 摧毁后资源
            if (c.ResourcesDestroyed.Count == 0)
            {
                NaturalResourcesDestroyedTextBlock.Visibility = Visibility.Collapsed;
                NaturalResourcesDestroyedStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                _creatureNestResourcesDestroyedListStringList = new List<List<string>>();
                if (c.ResourcesDestroyed.Count == 1)
                {
                    for (var i = 0; i <= _creatureNestMaxIndex; i++)
                        _creatureNestResourcesDestroyedListStringList.Add(c.ResourcesDestroyed[0]);
                }
                else
                {
                    foreach (var strList in c.ResourcesDestroyed)
                    {
                        _creatureNestResourcesDestroyedListStringList.Add(strList);
                    }
                }
                ShowResourcesDestroyed(0);
            }
            // ***中空树桩额外掉落***
            if (c.EnName == "Hollow Stump")
            {
                ShowHollowStumpDrops();
            }
            // ***蜜蜂女王摧毁后资源->资源***
            if (c.EnName == "Gigantic Beehive")
            {
                NaturalResourcesDestroyedTextBlock.Text = "资源：";
            }
            //特殊能力
            if (c.Ability.Count == 0)
            {
                NaturalAbilityTextBlock.Visibility = Visibility.Collapsed;
                NaturalAbilityStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                _creatureNestAbilityListStringList = new List<List<string>>();
                if (c.Ability.Count == 1)
                {
                    for (var i = 0; i <= _creatureNestMaxIndex; i++)
                        _creatureNestAbilityListStringList.Add(c.Ability[0]);
                }
                else
                {
                    foreach (var strList in c.Ability)
                    {
                        _creatureNestAbilityListStringList.Add(strList);
                    }
                }
                ShowAbility(0);
            }
            // 主要生物群落
            var biomesThickness = new Thickness(2, 0, 2, 0);
            if (c.Biomes == null)
            {
                NaturalBiomesTextBlock.Visibility = Visibility.Collapsed;
                NaturalBiomesWrapPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NaturalBiomesTextBlock.Visibility = Visibility.Visible;
                NaturalBiomesWrapPanel.Visibility = Visibility.Visible;
                foreach (var str in c.Biomes)
                {
                    var picButton = new PicButton
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = biomesThickness,
                        Source = StringProcess.GetGameResourcePath(str),
                        PictureSize = 90
                    };
                    //picButton.Click += Creature_Jump_Click;
                    NaturalBiomesWrapPanel.Children.Add(picButton);
                }
            }
            // 介绍
            NatureIntroduction.Text = c.Introduction;
            // 控制台
            if (c.Console.Count != 0)
            {
                ConsolePre.Text = $"c_spawn(\"{c.Console[0]}\",";
                foreach (var console in c.Console)
                {
                    _creatureNestConsoleStringList.Add(console);
                }
                console = c.Console[0];
            }
            else
            {
                CopyGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 显示生物
        /// </summary>
        /// <param name="index"></param>
        private void ShowCreature(int index)
        {
            NaturalCreatureStackPanel.Children.Clear();
            if (_creatureNestCreatureListStringList[index].Count == 0)
            {
                NaturalCreatureTextBlock.Visibility = Visibility.Collapsed;
                NaturalCreatureStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NaturalCreatureTextBlock.Visibility = Visibility.Visible;
                NaturalCreatureStackPanel.Visibility = Visibility.Visible;
                var thickness = new Thickness(20, 0, 0, 0);
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = thickness
                };
                foreach (var creature in _creatureNestCreatureListStringList[index])
                {
                    var picButton = new PicButton
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Source = StringProcess.GetGameResourcePath(creature),
                        PictureSize = 75
                    };
                    stackPanel.Children.Add(picButton);
                }
                NaturalCreatureStackPanel.Children.Add(stackPanel);
            }
        }

        /// <summary>
        /// 显示摧毁后资源
        /// </summary>
        /// <param name="index">索引序号</param>
        private void ShowResourcesDestroyed(int index)
        {
            NaturalResourcesDestroyedStackPanel.Children.Clear();
            var thickness = new Thickness(20, 0, 0, 0);
            if (_creatureNestResourcesDestroyedListStringList[index].Count == 0)
            {
                NaturalResourcesDestroyedTextBlock.Visibility = Visibility.Collapsed;
                NaturalResourcesDestroyedStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NaturalResourcesDestroyedTextBlock.Visibility = Visibility.Visible;
                NaturalResourcesDestroyedStackPanel.Visibility = Visibility.Visible;
                foreach (var str in _creatureNestResourcesDestroyedListStringList[index])
                {
                    // 数量分割点
                    var breakPosition = str.IndexOf('|');
                    // 工具分割点
                    var toolBreakPosition = str.IndexOf('&');
                    // 图片
                    var resourceSource = str.Substring(0, breakPosition);
                    // 数量文本
                    var resourceText = str.Substring(breakPosition + 1, toolBreakPosition - breakPosition - 1);
                    // 工具
                    var toolText = str.Substring(toolBreakPosition + 1);
                    // 使用工具
                    if (toolText.Length > 0)
                    {
                        var stackPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Margin = thickness
                        };
                        if (resourceSource == "无")
                        {
                            var textBlock = new TextBlock
                            {
                                Text = "无（",
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            stackPanel.Children.Add(textBlock);
                        }
                        else if (!string.IsNullOrEmpty(resourceSource))
                        {
                            var picButton1 = new PicButton
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Source = StringProcess.GetGameResourcePath(resourceSource),
                                Text = resourceText + "（"
                            };
                            stackPanel.Children.Add(picButton1);
                        }
                        else if (string.IsNullOrEmpty(resourceSource))
                        {
                            var textBlock = new TextBlock
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = resourceText + "（"
                            };
                            stackPanel.Children.Add(textBlock);
                        }
                        PicButton picButton2;
                        PicButton picButton3;
                        if (toolText == "Shovel")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_shovel")
                            };
                            picButton3 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_goldenshovel"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                            stackPanel.Children.Add(picButton3);
                        }
                        else if (toolText == "Machete")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_machete")
                            };
                            picButton3 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_luxury_machete"),
                            };
                            var picButton4 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_obsidian_machete"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                            stackPanel.Children.Add(picButton3);
                            stackPanel.Children.Add(picButton4);
                        }
                        else if (toolText == "Axe")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_axe")
                            };
                            picButton3 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_goldenaxe"),
                            };
                            var picButton4 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_pickaxe_1"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                            stackPanel.Children.Add(picButton3);
                            stackPanel.Children.Add(picButton4);
                        }
                        else if (toolText == "Pickaxe")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_pickaxe")
                            };
                            picButton3 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_goldenpickaxe"),
                            };
                            var picButton4 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_pickaxe_1"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                            stackPanel.Children.Add(picButton3);
                            stackPanel.Children.Add(picButton4);
                        }
                        else if (toolText == "Gunpowder")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_gunpowder")
                            };
                            picButton3 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_coconade"),
                            };
                            var picButton4 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_obsidian_coconade"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                            stackPanel.Children.Add(picButton3);
                            stackPanel.Children.Add(picButton4);
                        }
                        else if (toolText == "Hammer")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_hammer"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                        }
                        else if (toolText == "Torch")
                        {
                            picButton2 = new PicButton
                            {
                                Source = StringProcess.GetGameResourcePath("S_Torch"),
                                Text = "）"
                            };
                            stackPanel.Children.Add(picButton2);
                        }
                        //picButton1.Click += Creature_Jump_Click;
                        //picButton2.Click += Creature_Jump_Click;
                        NaturalResourcesDestroyedStackPanel.Children.Add(stackPanel);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(resourceSource))
                        {
                            var picButton = new PicButton
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = thickness,
                                Source = StringProcess.GetGameResourcePath(resourceSource),
                                Text = resourceText
                            };
                            //picButton.Click += Creature_Jump_Click;
                            NaturalResourcesDestroyedStackPanel.Children.Add(picButton);
                            
                        }
                        else
                        {
                            var textBlock = new TextBlock
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = thickness,
                                VerticalAlignment = VerticalAlignment.Center,
                                Text = resourceText
                            };
                            NaturalResourcesDestroyedStackPanel.Children.Add(textBlock);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 显示中空树桩额外掉落表
        /// </summary>
        private void ShowHollowStumpDrops()
        {
            // 主StackPanel
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20, 10, 0, 0)
            };
            // 30%StackPanel
            var stackPanelP30 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var textBlockP30 = new TextBlock
            {
                Text = "30%：",
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanelP30.Children.Add(textBlockP30);
            var picButtonP30 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("F_morsel")
            };
            stackPanelP30.Children.Add(picButtonP30);
            stackPanel.Children.Add(stackPanelP30);
            // 20%StackPanel
            var stackPanelP20 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var textBlockP20 = new TextBlock
            {
                Text = "20%：",
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanelP20.Children.Add(textBlockP20);
            var picButtonP20 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("G_boneshard")
            };
            stackPanelP20.Children.Add(picButtonP20);
            stackPanel.Children.Add(stackPanelP20);
            // 5%StackPanel
            var stackPanelP5 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var textBlockP5 = new TextBlock
            {
                Text = "5%：  ",
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanelP5.Children.Add(textBlockP5);
            var picButtonP51 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("S_rope")
            };
            stackPanelP5.Children.Add(picButtonP51);
            var picButtonP52 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("G_azure_feather")
            };
            stackPanelP5.Children.Add(picButtonP52);
            var picButtonP53 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("G_crimson_feather")
            };
            stackPanelP5.Children.Add(picButtonP53);
            var picButtonP54 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("G_jet_feather")
            };
            stackPanelP5.Children.Add(picButtonP54);
            if (Global.GameVersion <= 1)
            {
                var picButtonP55 = new PicButton
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Source = StringProcess.GetGameResourcePath("G_saffron_feather")
                };
                stackPanelP5.Children.Add(picButtonP55);
            }
            stackPanel.Children.Add(stackPanelP5);
            // 2%StackPanel
            var stackPanelP2 = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            var textBlockP2 = new TextBlock
            {
                Text = "2%：  ",
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanelP2.Children.Add(textBlockP2);
            var picButtonP21 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("A_redbird")
            };
            stackPanelP2.Children.Add(picButtonP21);
            var picButtonP22 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("A_snowbird")
            };
            stackPanelP2.Children.Add(picButtonP22);
            var picButtonP23 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("A_crow")
            };
            stackPanelP2.Children.Add(picButtonP23);
            if (Global.GameVersion <= 1)
            {
                var picButtonP24 = new PicButton
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Source = StringProcess.GetGameResourcePath("A_canary")
                };
                stackPanelP2.Children.Add(picButtonP24);
            }
            var picButtonP25 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("A_rabbit")
            };
            stackPanelP2.Children.Add(picButtonP25);
            var picButtonP26 = new PicButton
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Source = StringProcess.GetGameResourcePath("A_moleworm")
            };
            stackPanelP2.Children.Add(picButtonP26);
            stackPanel.Children.Add(stackPanelP2);
            // 添加进NaturesRootGrid
            NaturesRootGrid.Children.Add(stackPanel);
            Grid.SetRow(stackPanel, 9);
        }

        /// <summary>
        /// 显示特殊能力
        /// </summary>
        /// <param name="index"></param>
        private void ShowAbility(int index)
        {
            NaturalAbilityStackPanel.Children.Clear();
            if (_creatureNestAbilityListStringList[index].Count == 0)
            {
                NaturalAbilityTextBlock.Visibility = Visibility.Collapsed;
                NaturalAbilityStackPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                NaturalAbilityTextBlock.Visibility = Visibility.Visible;
                NaturalAbilityStackPanel.Visibility = Visibility.Visible;
                var thickness = new Thickness(20, 0, 0, 0);
                foreach (var str in _creatureNestAbilityListStringList[index])
                {
                    var textBlock = new TextBlock
                    {
                        Text = str,
                        Margin = thickness
                    };
                    NaturalAbilityStackPanel.Children.Add(textBlock);
                }
            }
        }

        /// <summary>
        /// 左右切换按钮
        /// </summary>
        private void SwitchLeftButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchRightButton.IsEnabled = true;
            if (_creatureNestIndex != 0)
            {
                _creatureNestIndex -= 1;
                if (_creatureNestIndex == 0)
                {
                    SwitchLeftButton.IsEnabled = false;
                }
                NatureImage.Source = new BitmapImage(new Uri(_creatureNestList[_creatureNestIndex], UriKind.Relative));
                if (_creatureNestHealthIntList != null && _creatureNestHealthIntList.Count > 1)
                {
                    CreatureHealth.Value = _creatureNestHealthIntList[_creatureNestIndex];
                }
                if (_creatureNestCreatureListStringList != null)
                    ShowCreature(_creatureNestIndex);
                if (_creatureNestResourcesDestroyedListStringList != null)
                    ShowResourcesDestroyed(_creatureNestIndex);
                if (_creatureNestAbilityListStringList != null)
                    ShowAbility(_creatureNestIndex);
                if (_creatureNestConsoleStringList.Count != 1)
                {
                    ConsolePre.Text = $"c_spawn(\"{_creatureNestConsoleStringList[_creatureNestIndex]}\",";
                    console = _creatureNestConsoleStringList[_creatureNestIndex];
                }
            }
        }

        private void SwitchRightButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchLeftButton.IsEnabled = true;
            if (_creatureNestIndex != _creatureNestMaxIndex)
            {
                _creatureNestIndex += 1;
                if (_creatureNestIndex == _creatureNestMaxIndex)
                {
                    SwitchRightButton.IsEnabled = false;
                }
                NatureImage.Source = new BitmapImage(new Uri(_creatureNestList[_creatureNestIndex], UriKind.Relative));
                if (_creatureNestHealthIntList != null && _creatureNestHealthIntList.Count > 1)
                {
                    CreatureHealth.Value = _creatureNestHealthIntList[_creatureNestIndex];
                }
                if (_creatureNestCreatureListStringList != null)
                    ShowCreature(_creatureNestIndex);
                if (_creatureNestResourcesDestroyedListStringList != null)
                    ShowResourcesDestroyed(_creatureNestIndex);
                if (_creatureNestAbilityListStringList != null)
                    ShowAbility(_creatureNestIndex);
                if (_creatureNestConsoleStringList.Count != 1)
                {
                    ConsolePre.Text = $"c_spawn(\"{_creatureNestConsoleStringList[_creatureNestIndex]}\",";
                    console = _creatureNestConsoleStringList[_creatureNestIndex];
                }
            }
        }

        private void ConsoleNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = (TextBox)sender;
            StringProcess.ConsoleNumTextCheck(textbox);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConsoleNum.Text) || double.Parse(ConsoleNum.Text) == 0)
            {
                ConsoleNum.Text = "1";
            }
            Global.SetClipboard(Settings.CopySelfMode == false ? $"{ConsolePre.Text}{ConsoleNum.Text}{ConsolePos.Text}" : $"{console}");
        }

        private void Console_Click(object sender, RoutedEventArgs e)
        {
            SendKey.SendMessage(ConsolePre.Text + ConsoleNum.Text + ConsolePos.Text);
        }
    }
}
