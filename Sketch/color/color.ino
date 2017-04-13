#include <Adafruit_CircuitPlayground.h>
#include <Wire.h>
#include <math.h>

class RGB
{
  public:int red = 0;
  public:int green = 0;
  public:int blue = 0;
};

int incomingByte = 0;   // for incoming serial data

char val = 0;
int count = 0;
int oldcount = 0;
RGB row [5];
void setup() {
  // put your setup code here, to run once:
  CircuitPlayground.begin();
  Serial.begin(9600);
}

void loop() {
    oldcount = 0;
    if (Serial.available()) {             // If data is available to read,
      val = Serial.read();                // read it and store it in val
  

      if (val == 'T') {  
        while (!Serial.available()) {}
        count = Serial.read();
        oldcount = count;
        for (int i=0;i<count;i++)
        {
          while (!Serial.available()) {}    //Wait until next value.
          row[i].red = Serial.read();              //Once available, assign.

          while (!Serial.available()) {}    //Same as above.
          row[i].green = Serial.read();

          while (!Serial.available()) {}
          row[i].blue = Serial.read();
        }
      }
      //}
      //if (Serial.available()) {             // If data is available to read,
      //val = Serial.read();                // read it and store it in val
  

      if (val == 'B') {  
        while (!Serial.available()) {}
        count = Serial.read();
        oldcount = count;
        for (int i=count;i<count+oldcount;i++)
        {
          while (!Serial.available()) {}    //Wait until next value.
          row[i].red = Serial.read();              //Once available, assign.

          while (!Serial.available()) {}    //Same as above.
          row[i].green = Serial.read();

          while (!Serial.available()) {}
          row[i].blue = Serial.read();
        }
      }

      if (val == 'E') {  
        while (!Serial.available()) {}
        count = Serial.read();
        for (int i=0;i<count;i++)
        {
          while (!Serial.available()) {}    //Wait until next value.
          row[i].red = Serial.read();              //Once available, assign.

          while (!Serial.available()) {}    //Same as above.
          row[i].green = Serial.read();

          while (!Serial.available()) {}
          row[i].blue = Serial.read();
        }
      }
      }
    // Loop through each pixel and set it to the appropriate value.
    for(int i=0; i<count+oldcount; i++) {
      CircuitPlayground.strip.setPixelColor(i, row[i].red, row[i].green, row[i].blue);
    }
    // Show all the pixels.
    CircuitPlayground.strip.show();

}
