# Automotive K-Line wifi tester using ESP8266 for SAMAND ECU (SIEMENS VDO SIM2K 34VR). 
This project is aimed to establish a wifi communication link between vehicle's computer and a portable media device such as (PC laptop, android device, ...).
This work contains two major parts: 
1- The hardware adapter connected to the vehicle obd2 port.
2- The Software on the labtop that processes the data received over wifi from the hardware adapter.
The hardware adapter is based on the esp8266 microcontroller and it is connected to the vehicle obd2 port through a k-line driver over a serial connection.
At the same time the adapter  is connected with the consumer smart device (labtop, tablet, smart phone) by wifi connection.
when the adapter plugged into vehicles obd2 port, it will initialize a connection session with the vehicles ECU, and it will also establish a wifi access point, allowing available smart devices to be connected to the vehicles ECU.
