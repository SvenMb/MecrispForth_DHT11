# MecrispForth_DHT11
Forth words for DHT11 temperature and humidity module

This was written on an STM32f103, but should run on any STM32 microcontroller.

This is written for Mecrips-Stellaris Forth:
http://mecrisp.sourceforge.net/

And I used pin??.fs, io.fs and hal.fs from Jean-Claude Wippler's flib to simplify the hardware access. 
https://git.jeelabs.org/jcw/embello/src/branch/master/explore/1608-forth/flib/

Bugs:
- A lot is done via polling the GPIO-port, this should be changed to interupt driven

Be aware this is still experimental code.
