PB3 constant DHT11 \ Port am STM32 

: readDHT11

omode-pp DHT11 io-mode!
DHT11 ioc!
18 ms
DHT11 ios!
IMODE-FLOAT DHT11 io-mode!
40 us

DHT11 io@
 if
	-1 \ errorcode -1 \ (doesn't start phase 1)
	quit
 then
 
80 us 
DHT11 io@
 not if
	-2 \ errorcode -2 \ (doesn"t start phase 2)
	quit
 then
 
80 us

0
5 0 do
  0
  8 0 do
    begin DHT11 io@ 0<> until
      30 us
      dht11 io@ 0<> if 1 else 0 then
      7 I - lshift or
      1000 0 do
        DHT11 io@ 0= if leave then
        I 999 = if  \ errorcode -3 \ (no answer in transfer)
	  k 0 ?do drop loop \ cleanup stack
      	  -3 quit
        then
      loop
    loop
  dup rot + \ add up checksum      
loop

swap dup + - \ last one is Checksum 
0<> if 2drop 2drop -4 quit then \ errorcode -4 (checksum wrong)

swap

\ take care of negative temps
dup $80 and if
    $7f and negate
then

10 * +

\ humidity as last, since then a negative value can indicate error 
-rot
swap 10 * + 
;



: DHT11.
CR

dup 0< if 
  ." DHT Error# " . 
  quit
then

swap \ temp first

s>d swap over dabs <# # $2E hold #S rot sign #> type
." °C"

\ no negative values in humi
CR
0 swap 0 <# # $2E hold #S #> type
." % humidity"

;

