   M S B A M L   `   `   `  ����A  =WPFApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:WPFAppWPFApp  5   	       WPFApp.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:WPFApp     5   	     � Title$  Image Overlay Tool��5   	   $	��600��6$   $
��1000��61   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6    �� Orientation$ Vertical=�6%   $��10q�6<   .+�5      �� $��Список слоёв:��6   $
��Bold"�6-   $��14(�6?   $��0,0,0,5q�6M   �� 5      -   5         ��Name$ LayersListBox��5      $	��180��6-    �� DisplayMemberPath$ FileName��5       �� SelectedIndex$ 0��5      �� 5      $ Vertical=�6   $��0,10,0,0q�60   .+�5      �� -   5         ��Name$ AddLayerButton��5      $��Добавить слой��61   $	��150��6I   $��40��6U   $��0,0,0,10q�6a   $��14(�5      
����ح�
����  �$��1q�6U   �� 5      -   5        $ RemoveLayerButton��5      $��Удалить слой��64   $	��150��6K   $��40��6W   $��14(�5      
�������
���  ��$��1q�6U   5      �� 5      $��Прозрачность:��6   $��0,15,0,5q�6-   $��14(�6?   $
��Bold"�6M   �� 5      -   5         ��Name$ OpacitySlider��5       � Minimum$ 0Q�6,   	 � Maximum$	 1Q�68   
 � Value$
 1Q�6D    �� TickFrequency$	 0.1Q�5       �� IsSnapToTickEnabled$
 True��6)   $	��200��6D   �� 5!      $ Vertical=�6   $��0,15,0,0q�60   .+�5"      �� -   5        $ MergeButton��5"      $��Соединить��6.   $	��150��6B   $��40��6N   $��16(�5#      $
��Bold"�6'   
������
��� d �$��1q�6i   �� 5$      -   5        $ ExportButton��5$      $��Сохранить��6/   $	��150��6C   $��40��6O   $��0,10,0,0q�6[   $��16(�5%      $
��Bold"�6'   
����·�
����  �$��1q�6j   5&      �� 5(      -   5         ��Name$ 	ZoomLabel��5(   )   $��Зум: 100%��6   $��0,15,0,5q�6<   $��14(�6N   $
��Bold"�6\   5)      �� 5,   
   $��1��6   $��Stretch=�6'   $��Stretch=�6E   $
��Auto=�6a   $
��Auto=�5-      
�������.��5.      �� -   5         ��Name$ 
MainCanvas��5.      
�������53      54      55      56      