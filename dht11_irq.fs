\ Example DHT11 read program with interupt use
\ with DHT11 on PB3

\ needs io.fs from jcw
\ made for STM32F103C8 'blue pill'

\ data from RM0008 for STM32F10xxx
\ $40010000 constant afio
\ 4 bit defining the port PA -> %0000 PB-> %0001 ...
\ starting with exti0
afio $08 + constant EXTICR1
\ starting with exti5
\ afio $0c + constant EXTICR2
\ starting with exti8
\ afio $10 + constant EXTICR3
\ starting with exti12
\ afio $14 + constant EXTICR4

\ bits for enabling exti
afio $400 + constant EXTI_IMR
\ bits for raising edge irq
\ afio $408 + constant EXTI_RTSR
\ bits for falling edge irq
afio $40C + constant EXTI_FTSR
\ irq status, also for irq reset
afio $414 + constant EXTI_PR

\ documented in RM0008 for STM32F10xxx
\ table 61 Vector table for connectivity line devices
\ from position 32 it starts with NVIC_ISER1
\ $E000E100 constant NVIC_ISER0 
\ NVIC_ISER0 $4 + constant NVIC_ISER1 
\ 6 constant EXTI0_irq 
\ 7 constant EXTI1_irq 
9 constant EXTI3_irq

0 variable DHT11Time
6 buffer: dht11data


: dht11.
    CR
    
    dht11time @ drop \ first read get garbage, don't know why
    dht11data 5 + c@ if
	." Error"
	exit
    then

    dht11data 2 + c@ dup $80 and if
	$7f and negate
    then
    10 *
    dht11data 3 + c@ +
    s>d swap over dabs <# # $2E hold #S rot sign #> type
    ." °C"

    CR

    dht11data c@ 10 *
    dht11data 1+ c@ +
    0 <# # $2E hold #S #> type
    ." % humidity"
;


\ irq service routine for IR input
\ very simple not much error prone, but works for me :)
: dht11_isr
    3 bit EXTI_PR bit@ not if exit then  \ exit wenn nicht exti3
    micros DHT11Time @ over DHT11Time ! -  \ get diff to last irq
    case
	dup 70 - 20 < ?of
	    DHT11data 5 + c@ 8 /mod \ which bit
	    DHT11data +
	    swap 7 - abs bit swap
	    cbic! \ clear bit
	endof 
	dup 110 - 20 < ?of
	    DHT11data 5 + c@ 8 /mod \ which bit
	    DHT11data +
	    swap 7 - abs bit swap
	    cbis! \ set bit
        endof
	DHT11data 5 + c@ $f0 >= not if \ wrong timing after start
	    ." Error: Bit" DHT11data 5 + c@ .
	    39 DHT11data 5 + c! \ this means the end...
	then
    endcase
    DHT11data 5 + dup c@ \ actual bit
    1+ dup rot c! \ one more, and keep the number
    40 = if \ last bit read
	0
	dht11data dup 4 + swap do
	    i c@ +
	loop
	dht11data 4 + c@
	<> if
	    ." checksum wrong"
	    dht11data 6 $ff fill
	else
	    0 dht11data 5 + c!
	    0 dht11time !
	then
	3 bit EXTI_IMR bic!  \ disable exti3
	EXTI3_irq bit nvic_iser0 bic! \ disable exti3 in nvic
    then
    
    3 bit EXTI_PR bis! \ clear exti3
;

\ setup and print DHT11 data
: DHT11@
    -2 DHT11data 5 + c!
    $E000 EXTICR1 bic!   \ PB3 for exti3
    $1000 EXTICR1 bis!   \ PB3 for exti3
    3 bit EXTI_FTSR bis! \ falling edge for exti3
    ['] dht11_isr irq-exti3 ! \ set isr for exti3
    omode-pp PB3 io-mode!
    PB3 ioc!
    18 ms \ maybe do it via timer? But it is at least possible to get irqs
    PB3 ios!
    micros DHT11Time !
    IMODE-FLOAT PB3 io-mode!

    3 bit EXTI_IMR bis!  \ enable exti3
    EXTI3_irq bit nvic_iser0 bis! \ enable exti3 in nvic
    10 ms \ wait and let the isr do the magic work
;
