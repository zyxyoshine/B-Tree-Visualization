   M S B A M L   `   `   `  ����J  FWpfApplication2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:WpfApplication2WpfApplication2  5   	        WpfApplication2.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:WpfApplication2     5   	     � Title$  B-Tree Visualization��5   	   $��342.047��6&   $��614.79��67    � 	Resources 5	      ��%    5
      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��B-Tree Visualization��6   $��Center=�6<   $��21.333(�6Y   �� 5   
   -   5         ��Name$ canvas��5      $��0,20,0,5q�6B   $��1��6T   5      �� 5   
   $��3��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   .+�5      �� $ 
Horizontal=�6   $��
15,5,15,15q�62   .+�5      �� -   5         ��Name$ InsertInputBox��5      $��20��62    �� TextWrapping$
 Wrap=�6?   $�� ��6S   $��50��6[   ���	#FFABADB3$��0,0,7,0q�6�   �� 5      -   5         ��Name$ InsertButton��5      $��Insert��6/   $��61��6@   $��20��6K   6W   5      �� 5      $ 
Horizontal=�6   $��
15,5,15,15q�62   .+�5      �� -   5        $ DeleteInputBox��5      $��20��62   $
 Wrap=�6>   $�� ��6R   $��50��6Z   ���	#FFABADB3$��0,0,7,0q�6�   �� 5      -   5        $ DeleteButton��5      $��Delete��6/   $��61��6@   $��20��6K   6W   5      �� 5      $ 
Horizontal=�6   $��
15,5,15,15q�62   .+�5      �� -   5        $ QueryInputBox��5      $��20��61   $
 Wrap=�6=   $�� ��6Q   $��50��6Y   ���	#FFABADB3$��0,0,7,0q�6�   �� 5      -   5        $ QueryButton��5      $��Query��6.   $��61��6>   $��20��6I   6U   5       5!      5"      5#      