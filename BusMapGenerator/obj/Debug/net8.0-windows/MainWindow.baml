   M S B A M L   `   `   `  ����\  XSkiaSharp.Views.WPF, Version=3.119.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756V>clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPFSkiaSharp.Views.WPF  5       BusMapGenerator    BusMapGenerator.MainWindow   5      R NAvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4` \AvalonDock.Themes.VS2013, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35L 9http://schemas.microsoft.com/winfx/2006/xaml/presentation        5      R	 NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml   	  5      @d2http://schemas.microsoft.com/expression/blend/2008   	  5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006   	  5      N
 JFluent, Version=10.0.0.0, Culture=neutral, PublicKeyToken=3e436e32a8c5546fFluenturn:fluent-ribbon 
 5      ;
AvalonDock(https://github.com/Dirkster99/AvalonDock   5      Kskia>clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF   5      D @ModernWpf, Version=0.9.6.0, Culture=neutral, PublicKeyToken=nullM IModernWpf.Controls, Version=0.9.6.0, Culture=neutral, PublicKeyToken=null,ui!http://schemas.modernwpf.com/2019   5        � Title$*  $BMG - 公共汽车线路图生成器��5
      $	��800��5      $	��600��5      0  *ModernWpf.Controls.Primitives.WindowHelper   UseModernWindowStyle$ False��5       � WindowState$ 	Maximized=�5       � WindowStartupLocation$ CenterScreen=�5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
    
 Fluent.Ribbon  $��0��6   $��13(�6%      Menu 5        
 Fluent.Backstage  5!      $��
0, 0, 7, 0q�6#    
 Fluent.RibbonControl   Header$ 工程��67      Content. 5"        
 Fluent.BackstageTabControl  .@�5#       
 Fluent.BackstageTabItem     Header$	 New��62     5$      $ Print��62    
 Fluent.Button  5%      	   Header$		 Hey��6(   
   Icon$8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��65   5&      5'         Tabs. 5+       
 Fluent.RibbonTabItem     Header$ 好玩��6#      ReduceOrder$� �Default,Default,Default,                                                 Large,Large,Large,                                                 Other,Other,Other��6/   	 
 Fluent.KeyTip 	  Keys$ P��5-   D      Groups. 5.      
 
 Fluent.RibbonGroupBox
  $��Group��6(   .@�5/        -   5        $��button1��5/   $   $	 生成��63   $8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6?      	LargeIcon$8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6y   $ T��6�   $	��Top=�6�   $��
3, 3, 3, 1q�6�   6�     50      -   5        $��button2��50   $   $	 按钮2��63   $8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6@   $8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6z      SizeDefinition/ 
 )Fluent.Converters.SizeDefinitionConverter$ Middle 6�   $ H��6�   $��
3, 3, 3, 1q�6�     51      -   5        $��button3��51   $   $	 按钮3��63   $8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6@   $8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6z   $ Middle 6�   $��
3, 1, 3, 1q�6�     52      -   5        $��button4��52   $   $	 按钮4��63   $8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6@   $8 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��6z   $ Middle 6�   $��
3, 1, 3, 1q�6�   53      54         QuickAccessItems 57        
 Fluent.QuickAccessMenuItem  59       m� 	IsChecked$
 true��6-      Target. 5:        -   5        $
	 Pink��5:   $   $8
 2https://s2.loli.net/2024/08/13/YTpbrEMkNcSWmH2.png��62   $ 1��6l   $��
2, 2, 2, 2q�6�   6�   5;        5=       6-   ��  �� ElementName$ button1��5@        AvalonDock.DockingManager  5C   
   -   5          Name$ myDockManager��5C   $   $��1��6;      DocumentHeaderTemplate 5F      �� 5G      .R�5H      ��  �� Orientation$ 
Horizontal=�6!   $��60��6:   .+�5I      �� $��0,0,4,0q�6    ��61   �� *
IconSource+�� 5J      $��CharacterEllipsis=�6;   ��6$   �� *Title+6;   5K      5L         Layout. 5P      "  AvalonDock.Layout.LayoutRoot     	RootPanel. 5R      #  AvalonDock.Layout.LayoutPanel     Orientation$ 
Horizontal=�6)   �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutPanelElement, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 5T      1  +AvalonDock.Layout.LayoutAnchorablePaneGroup  �  �AvalonDock.Layout.LayoutPositionableGroup`1[[AvalonDock.Layout.ILayoutAnchorablePane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   	DockWidth$	 260 �6;   �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutAnchorablePane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 5U      ,  &AvalonDock.Layout.LayoutAnchorablePane  �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.LayoutAnchorable, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]   Children. 5V      (  "AvalonDock.Layout.LayoutAnchorable  -   5        
WPFWindow1��%  AvalonDock.Layout.LayoutContent    Title$  一个左侧窗格��5W   #   !   CanClose$
! True��5X   #   "   CanHide$" False��5Y   #   #   	ContentId$# 
WPFWindow1��5Z   #   $   ToolTip$$ 
WPF 组件��5[   #   5\        5]      -   5        
WPFWindow2��$  另一个左侧窗格��5^   #   $
! True��5_   #   $" False��5`   #   $# 
WPFWindow2��5a   #   $$ 
WPF 组件��5b   #   %   Content.% 5c   "   �� $��玩它��6*   5d      5e      5f      /  )AvalonDock.Layout.LayoutDocumentPaneGroup  5h      �  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.ILayoutDocumentPane, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]&   Children.& 5i      *  $AvalonDock.Layout.LayoutDocumentPane  -	   5        LayoutDocumentPane���  �AvalonDock.Layout.LayoutGroup`1[[AvalonDock.Layout.LayoutContent, AvalonDock, Version=4.72.1.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4]]'   Children.' 5j      &   AvalonDock.Layout.LayoutDocument  -
   5        
Opener��$  欢迎��5j   H   $# Opener��6S   $! False��6f   .% 5l   "   �  5m   &   �� 5n   *   $��3* �68   �� 5o   *   $��* �68   �� 5p   *   $��* �68   �� 5q   *   $��4* �68   .+�5s   &   �� $��1��6,   $:��4欢迎使用 BMG - 公共汽车线路图生成器！��69   $��Center=�6Z   $��Center=�6u   $��20(�6�   �� 5t   &   -   5        $��2��5t   -   $��打开地图��6:   $��Center=�6Y   $��Center=�6v   $��26(�6�   5u   #   5v        5w      -   5        RoadPreviewer��$  道路预览��5w   O   $# RoadPreviewer��6\   $! False��6v   .% 5y   "   #   SkiaSharp.Views.WPF.SKElement  -   5        (  Name$( 
SkiaCanvas��5y   1   5~   %   5        5�      -   5        
Mapper��$  	线路图��5�   H   $# Mapper��6T   $! False��6g   .% 5�   "   �  5�   &   �� 5�   *   �� 5�   *   $
��Auto �68   .+�5�   &   �� $��28��6-   $��点击添加 2 个文档��69   �� 5�   &   $��1��6.   $��文档 1 内容��6;   5�   #   5�      5�      5�        5�      $	 260 �6;   . 5�        . 5�        -   5        
WPFWindow4��$  右侧窗格��5�      $
! True��5�      $" False��5�      $# 
WPFWindow4��5�      $$ 
WPF 组件��5�      .% 5�   "   �  5�   &   �� 5�   *   $��* �68   �� 5�   *   $��* �68   .+�5�   &   �� -   5        ) ��Name$) textBox3��5�   .   $��右侧窗格内容3��6@   $��0��6O   �� 5�   &   $��右侧窗格内容2��6.   $��1��6=   5�   #   5�      5�      5�      5�      5�      *   Theme* 5�      (  "AvalonDock.Themes.Vs2013LightTheme  5�      5�       
 Fluent.StatusBar  5�   
   $��2��6   .@�5�        
 Fluent.StatusBarItem   +    Title$+ 	左侧项��6#   ,    Value$	, 150��5�      $
��Left=�5�      .��5�      �� $��150 px��6   5�      �� 5�      $
��Left=�6      5�      $+ 第二个左侧项��6#   $!, 值会显示在右键菜单��5�      - � ToolTip$- 这是一个提示��5�      $'��!这个内容会显示在状态栏��5�      $
��Left=�5�         5�      $+ 	右侧项��6#   $��Right=�5�      $?, 9如果没有设置 Content，则显示 Value 作为内容��5�      5�      5�      5�      