   M S B A M L   `   `   `  ���� \  XSkiaSharp.Views.WPF, Version=3.119.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756V>clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPFSkiaSharp.Views.WPF   BusMapGenerator    BusMapGenerator.MainWindow   R NAvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4` \AvalonDock.Themes.VS2013, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35L 9http://schemas.microsoft.com/winfx/2006/xaml/presentation        R	 NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml   	  @d2http://schemas.microsoft.com/expression/blend/2008   	  Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006   	  N
 JFluent, Version=10.0.0.0, Culture=neutral, PublicKeyToken=3e436e32a8c5546fFluenturn:fluent-ribbon 
 ;
AvalonDock(https://github.com/Dirkster99/AvalonDock   Kskia>clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF   D @ModernWpf, Version=0.9.6.0, Culture=neutral, PublicKeyToken=nullM IModernWpf.Controls, Version=0.9.6.0, Culture=neutral, PublicKeyToken=null,ui!http://schemas.modernwpf.com/2019     � Title$*  $BMG - 公共汽车线路图生成器��$	��800��$	��600��0  *ModernWpf.Controls.Primitives.WindowHelper   UseModernWindowStyle$ False�� � WindowState$ 	Maximized=� � WindowStartupLocation$ CenterScreen=�.���  � RowDefinitions �� $
��Auto ��� $��* ��� $
��Auto �.+� 
 Fluent.Ribbon  $��0��$��13(�   Menu  
 Fluent.Backstage  $��
0, 0, 7, 0q� 
 Fluent.RibbonControl   Header$ 工程��   Content.   
 Fluent.BackstageTabControl  .@� 
 Fluent.BackstageTabItem     Header$	 New��  $ Print�� 
 Fluent.Button  	   Header$		 Hey��
   Icon$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��   Tabs.  
 Fluent.RibbonTabItem     Header$ 好玩��   ReduceOrder$� �Default,Default,Default,                                                 Large,Large,Large,                                                 Other,Other,Other��	 
 Fluent.KeyTip 	  Keys$ P��   Groups. 
 
 Fluent.RibbonGroupBox
  $��Group��.@�  -   $��button1��$	 按钮1��$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��   	LargeIcon$8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$ T��$	��Top=�$��
3, 3, 3, 1q�  -   $��button2��$	 按钮2��$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��   SizeDefinition/ 
 )Fluent.Converters.SizeDefinitionConverter$ Middle $ H��$��
3, 3, 3, 1q�  -   $��button3��$	 按钮3��$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$ Middle $��
3, 1, 3, 1q�  -   $��button4��$	 按钮4��$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$ Middle $��
3, 1, 3, 1q�   QuickAccessItems   
 Fluent.QuickAccessMenuItem   m� 	IsChecked$
 true��   Target.   -   $
	 Pink��$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��$ 1��$��
2, 2, 2, 2q�   ��  �� ElementName$ button1��  AvalonDock.DockingManager  -     Name$ myDockManager��$��1��   DocumentHeaderTemplate �� .R���  �� Orientation$ 
Horizontal=�$��60��.+��� $��0,0,4,0q����� *
IconSource+�� $��CharacterEllipsis=����� *Title+   Layout. "  AvalonDock.Layout.LayoutRoot     	RootPanel. #  AvalonDock.Layout.LayoutPanel     Orientation$ 
Horizontal=��  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutPanelElement, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 1  +AvalonDock.Layout.LayoutAnchorablePaneGroup  �  �AvalonDock.Layout.LayoutPositionableGroup`1[[AvalonDock.Layout.ILayoutAnchorablePane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   	DockWidth$	 260 ��  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutAnchorablePane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. ,  &AvalonDock.Layout.LayoutAnchorablePane  �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.LayoutAnchorable, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. (  "AvalonDock.Layout.LayoutAnchorable  -   
WPFWindow1��%  AvalonDock.Layout.LayoutContent    Title$  一个左侧窗格��!   CanClose$
! True��"   CanHide$" False��#   	ContentId$# 
WPFWindow1��$   ToolTip$$ 
WPF 组件��  -   
WPFWindow2��$  另一个左侧窗格��$
! True��$" False��$# 
WPFWindow2��$$ 
WPF 组件��%   Content.% �� $��玩它��/  )AvalonDock.Layout.LayoutDocumentPaneGroup  �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutDocumentPane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]&   Children.& *  $AvalonDock.Layout.LayoutDocumentPane  -	   LayoutDocumentPane���  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.LayoutContent, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]'   Children.' &   AvalonDock.Layout.LayoutDocument  -
   
Opener��$  欢迎��$# Opener��$! False��.% �  �� $��3* ��� $��* ��� $��* ��� $��4* �.+��� $��1��$:��4欢迎使用 BMG - 公共汽车线路图生成器！��$��Center=�$��Center=�$��20(��� -   $��2��$��打开地图��$��Center=�$��Center=�$��26(�  -   RoadPreviewer��$  道路预览��$# RoadPreviewer��$! False��.% #   SkiaSharp.Views.WPF.SKElement  -   (  Name$( 
SkiaCanvas��  -   
Mapper��$  	线路图��$# Mapper��$! False��.% �  �� �� $
��Auto �.+��� $��28��$��点击添加 2 个文档���� $��1��$��文档 1 内容��  $	 260 �.   .   -   
WPFWindow4��$  右侧窗格��$
! True��$" False��$# 
WPFWindow4��$$ 
WPF 组件��.% �  �� $��* ��� $��* �.+��� -   ) ��Name$) textBox3��$��右侧窗格内容3��$��0���� $��右侧窗格内容2��$��1��*   Theme* (  "AvalonDock.Themes.Vs2013LightTheme   
 Fluent.StatusBar  $��2��.@�  
 Fluent.StatusBarItem   +    Title$+ 	左侧项��,    Value$	, 150��$
��Left=�.���� $��150 px���� $
��Left=�   $+ 第二个左侧项��$!, 值会显示在右键菜单��- � ToolTip$- 这是一个提示��$'��!这个内容会显示在状态栏��$
��Left=�   $+ 	右侧项��$��Right=�$?, 9如果没有设置 Content，则显示 Value 作为内容��