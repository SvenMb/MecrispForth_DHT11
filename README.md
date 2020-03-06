# MecrispForth_DHT11
Forth words for DHT11 temperature and humidity module

First version was done for a LED-clock with MQTT interface. Interface will be changed to be more general later. 

Bugs:
- If DHT11 is not connected, it sometimes hang but not always.
- A lot is done via polling the GPIO-port, this should be changed. 

Be aware this is very experimental code!
