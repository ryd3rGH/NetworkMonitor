   M S B A M L   `   `   `  ����a  ]Ryd3rNetworkMonitor.ClientControlPanel, Version=1.1.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:Ryd3rNetworkMonitor.ClientControlPanel&Ryd3rNetworkMonitor.ClientControlPanel  5   	   ?    9Ryd3rNetworkMonitor.ClientControlPanel.ControlPanelWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Flocal4clr-namespace:Ryd3rNetworkMonitor.ClientControlPanel     5   	   -   5          � Title$,  &Network Monitor - Client Control Panel��5   	   $	��550��68   $	��600��6E   .��5	      �  � ColumnDefinitions 5
   
   �� 5      $
��0.6* �6   �� 5       � RowDefinitions 5   
   �� 5      �� 5      $
��0.7* �6   .+�5   
   �� -   5         ��Name$ 
controlBox��5      $��Control��6'   $��
15,15,5,10q�68   $��2��6L   .��5      �  5      �� 5      �� 5      .+�5      ��  �� Orientation$ Vertical=�6   $��Stretch=�64   $	��Top=�6R   .+�5      �� -   5         ��Name$ startBtn��5      $��Start messaging��6/   $��10q�6I   $��25��6U   6a   �� 5      -   5        $ stopBtn��5      $��Stop messaging��6.   $��
10,0,10,10q�6G   $��25��6[   6g   �� 5      -   5        $ settingsBtn��5      $��Settings��62   $��
10,0,10,10q�6E   $��25��6Y   6e   5      �� 5      $ Vertical=�6   $��Stretch=�64   $��Bottom=�6R   $��1��6m   .+�5      � $��	10,0,10,0q�6    5      �� 5       $��2* �6/   �� 5!       5#      �� 5$      �� 5%      .+�5'      �� -   5         ��Name$ mesLbl��5'       $��Messaging status:��60   $��Right=�6L   $��0��6h   �� 5(      -   5        $ mesValueLbl��5(       $
��Left=�65   $��1��6P   $��0��6`   �� 5)      -   5        $ regLbl��5)       $��Registration status:��60   $��1��6O   $��Right=�6\   �� 5*      -	   5        $ regValueLbl��5*       $
��Left=�65   $��1��6P   $��1��6`   5+      5,      5-      5.      �� 5/   
   -
   5        $ infoBox��5/      $��Information��6$   $��1��69   $��0��6I   $��10,15,15,10q�6V   .��50      �  51      �� 52      $
��0.5* �6'   �� 53       55      �� 56      �� 57      �� 58      �� 59      �� 5:      �� 5;      .+�5=      �� -   5        $ ipLbl��5=      $��
Server IP:��6'   $��Center=�6<   $��Right=�6W   $��0,0,5,0q�6s   �� 5>      -   5        $ nameLbl��5>      $��
Full name:��6)   $��Center=�6>   $��Right=�6Y   $��0,0,5,0q�6u   $��1��6�   �� 5?      -   5        $ loginLbl��5?      $��Login:��6*   $��Center=�6;   $��Right=�6V   $��0,0,5,0q�6r   $��2��6�   �� 5@      -   5        $ 
printerLbl��5@      $��Printer or MFP:��6,   $��Center=�6F   $��Right=�6a   $��0,0,5,0q�6}   $��3��6�   �� 5A      -   5        $ upsLbl��5A      $��UPS availability:��6(   $��Center=�6D   $��Right=�6_   $��0,0,5,0q�6{   $��4��6�   �� 5B      -   5        $ scanLbl��5B      $��Scan availability:��6)   $��Center=�6F   $��Right=�6a   $��0,0,5,0q�6}   $��5��6�   �� 5C      -   5        $ 
ipValueLbl��5C      $��Center=�6,   $
��Left=�6G   $��5,0,0,0q�6b   $��1��6s   �� 5D      -   5        $ nameValueLbl��5D      $��Center=�6.   $
��Left=�6I   $��5,0,0,0q�6d   $��1��6u   $��1��6�   �� 5E      -   5        $ loginValueLbl��5E      $��Center=�6/   $
��Left=�6J   $��5,0,0,0q�6e   $��1��6v   $��2��6�   �� 5F      -   5        $ printerValueLbl��5F      $��Center=�61   $
��Left=�6L   $��5,0,0,0q�6g   $��1��6x   $��3��6�   �� 5G      -   5        $ upsValueLbl��5G      $��Center=�6-   $
��Left=�6H   $��5,0,0,0q�6c   $��1��6t   $��4��6�   �� 5H      -   5        $ scanValueLbl��5H      $��Center=�6.   $
��Left=�6I   $��5,0,0,0q�6d   $��1��6u   $��5��6�   5I      5J      �� 5K   
   -   5        $ logBox��5K      $	��Log��6#   $��1��60   $��1��6@   $��10, 0, 15, 10q�6M   .��5L      � $��Stretch=�6   $��Stretch=�61   .+�5M      �� -   5         ��Name$ logTxt��5M      $��Stretch=�6*   $��Stretch=�6F   " �� VerticalScrollBarVisibility$ Visible=�6d   
������ 	 �� 
IsReadOnly	 . $��Consolas)�6�   $
��10.5(�6�   6�   5N      5O      5P      5Q      