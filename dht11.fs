( start dht11: ) here hex.
\ define Port am STM32, can be any port 
PB3 constant DHT11



: DHT11@ ( - - temp humi )
\ returns errrorcode -1 - -4 in case of error
\ temperatur will be in 1/10 of degree celsius
\ humidity will be in promille


  \ send start signal to DHT11
  omode-pp DHT11 io-mode!
  DHT11 ioc!
  18 ms
  DHT11 ios!
  IMODE-FLOAT DHT11 io-mode!
  40 us

  \ check for ack
  DHT11 io@ if 
    -1 \ errorcode -1 \ (doesn't start phase 1)
    exit
  then
 
  80 us 
  DHT11 io@ not if
    -2 \ errorcode -2 \ (doesn't start phase 2)
    exit
  then
 
  80 us

  0 
  5 0 do \ 5 bytes from DHT11
    0 \ for checksum in loop
    8 0 do \ 8 bit each byte :)
      begin DHT11 io@ 0<> until
        30 us
        dht11 io@ 0<> if 1 else 0 then \ bit received
        7 I - lshift or \ put it in place
        1000 0 do  \ wait for next bit
          DHT11 io@ 0= if leave then
          I 999 = if  \ errorcode -3 \ (no start of next bit in transfer)
	    k 0 ?do drop loop \ cleanup stack
      	    -3 exit
          then
        loop
      loop
    dup rot + \ add up checksum      
  loop

  swap dup + - \ last one is Checksum 
  0<> if 2drop 2drop -4 exit then \ errorcode -4 (checksum wrong)

  swap

  \ take care of negative temps
  dup $80 and if
    $7f and negate
  then

  10 * + \ make it 1/10 of degree

  \ humidity as last, since then a negative value can indicate error 
  -rot  \ put temp behind the humi values
  swap 10 * + \ make it promille
;


: DHT11. ( temp humi - - )
\ prints out human readable values from DHT11@ 
\ checks if errorcode is on stack
\ temp is in 1/10 of degrees
\ humi is in promille

  CR

  dup 0< if
    drop \ clean stack
    ." DHT Error# " . 
    exit
  then

  swap \ temp first and take care of negative values

  s>d swap over dabs <# # $2E hold #S rot sign #> type
  ." °C"

  CR

  \ no negative values in humi
  0 <# # $2E hold #S #> type
  ." % humidity"
;

( end dht11: ) here hex.

