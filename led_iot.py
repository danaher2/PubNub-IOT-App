# This script requires the PubNub python package, which is only available for Python 2
# This script can be run on your Raspberry Pi
# To match the functionality, attach LEDs to the GPIO pins indicated in the pins dictionary below
# Fill in PubNub publish and subscribe keys below

from pubnub.pnconfiguration import PNConfiguration
from pubnub.callbacks import SubscribeCallback
from pubnub.pubnub import PubNub
from pubnub.enums import PNStatusCategory
import RPi.GPIO as GPIO
 
pnconfig = PNConfiguration()
pnconfig.subscribe_key = "!!!!FILL IN SUBSCRIBE KEY!!!!"
pnconfig.publish_key = "!!!!FILL IN PUBLISH KEY!!!!"
pnconfig.ssl = False
 
pubnub = PubNub(pnconfig)

#def publish_callback(result, status):
#    print(result)
    # handle publish result, status always present, result if successful
    # status.isError to see if error happened

#pubnub.publish().channel("Channel-xy9gcqgrs").message(["hello", "there"])\
#        .async(publish_callback)


GPIO.setmode(GPIO.BCM)

# Create a dictionary called pins to store the pin number, name, and pin state:
pins = {
    23 : {'name' : 'yellow LED', 'state' : GPIO.LOW},
    24 : {'name' : 'red LED', 'state' : GPIO.LOW},
    25 : {'name' : 'blue LED', 'state' : GPIO.LOW}
    }

# Set each pin as an output and make it low:
for pin in pins:
   GPIO.setup(pin, GPIO.OUT)
   GPIO.output(pin, GPIO.LOW)

# For each pin, read the pin state and store it in the pins dictionary:
for pin in pins:
    pins[pin]['state'] = GPIO.input(pin)

def toggle(changePin):
   # Convert the pin from the URL into an integer:
   changePin = int(changePin)
   GPIO.output(changePin, not GPIO.input(changePin))

class MyListener(SubscribeCallback):
    def status(self, pubnub, status):
        if status.category == PNStatusCategory.PNConnectedCategory:
            pass
            #pubnub.publish().channel("Channel-kz4mchwed").message({'fieldA': 'awesome', 'fieldB': 10}).sync()
 
    def message(self, pubnub, message):
        try:
            print('Message: ' + message.message)
            print('Pin: ' + message.message[u'pin'])
            changePin = int(message.message[u'pin'])
            toggle(changePin)
        except:
            pass
        
    def presence(self, pubnub, presence):
        pass
 
my_listener = MyListener()
 
pubnub.add_listener(my_listener)
 
pubnub.subscribe().channels("Channel-kz4mchwed").execute()
