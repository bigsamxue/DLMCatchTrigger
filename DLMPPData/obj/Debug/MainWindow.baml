   M S B A M L   `   `   `  ����D  @DLMPPData, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:DLMPPData	DLMPPData  5   	       DLMPPData.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:DLMPPData     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � RowDefinitions 5
   
   �� 5      $	��110 �6   �� 5      $	��120 �6   �� 5      .+�5   
   �� .+�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $"��IP地址                    ��6   $��30(�6<   $��40��6J   �� 5      -   5         ��Name$ ipaddr��5      $��192.168.1.254��6*   $��30(�6?   $	��250��6M   5      �� 5      $��20��6   �� 5      -   5         ��Name$ Button_Connect��5      $��连接��6-   $��30(�6:   6H   5      �� 5   
   $��1��6   $ 
Horizontal=�6"   .+�5      �� .+�5      �� $ 
Horizontal=�6   .+�5      �� $��CH   ��6    $��Center=�6-   $��Center=�6H   $��30(�6e   �� 5      -   5         ��Name$ chbox1��5       �� SelectedIndex$ 1��6/   $	��180��6A   $��50��6M   $��5,5,10,5q�6Y   $��30(�6k   �� 5      -   5        $ chbox2��5      $ 3��6/   $	��180��6A   $��50��6M   $��5,5,10,5q�6Y   $��30(�6k   �� 5      $��	MathCH   ��6    $��Center=�61   $��Center=�6L   $��30(�6i   �� 5      -   5        $ 	mathchbox��5      $ 0��62   $	��180��6D   $��50��6P   $��30(�6\   5      �� 5       $��15��6   �� 5!      -   5        $ Button_RunStop��5!      $��	 Run/Stop��61   $��30(�6E   6S   5"      5$      &   System.Windows.Controls.DataGrid  5%   
   $��2��6      CanUserAddRows .  $��30(�67      Columns 5&      0  *System.Windows.Controls.DataGridTextColumn  5'      ,  &System.Windows.Controls.DataGridColumn	   Header$	  ��6%   
   Width5  /System.Windows.Controls.DataGridLengthConverter$
 * 60     5(      $
	 Umax��6%   $
 * 63     5)      $
	 Umin��6%   $
 * 63     5*      $
	 Imax��6%   $
 * 63     5+      $
	 Imin��6%   $
 * 63     5,      $
	 Pmax��6%   $
 * 63     5-      $
	 Pmin��6%   $
 * 63   5/      50      51      