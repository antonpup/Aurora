const int greenPin = 10;  // LED connected to digital pin 9
const int redPin = 9;     // LED connected to digital pin 10
const int bluePin = 11;   // LED connected to digital pin 11

void setup() {
  // initialize serial communications at 115200 bps:
  Serial.begin(115200);

  // set the pins as output for your rgb
  pinMode(greenPin, OUTPUT);
  pinMode(redPin, OUTPUT);
  pinMode(bluePin, OUTPUT);

  //Set the pwm frequency to be a lot higher, prevents audible noise from your power supply
  setPwmFrequency(9, 1); //pin + divisor, default divisor = 8
  setPwmFrequency(11, 1);
}

void loop() {
  // read all serial data that has been send
  while (Serial.available() > 0) {

    // wait for int in the serial stream
    int red = Serial.parseInt();
    int green = Serial.parseInt();
    int blue = Serial.parseInt();

    // look for the newline. That's the end of the message
    if (Serial.read() == '\n') {
      // constrain the values to 0 - 255
      red = constrain(red, 0, 255);
      green = constrain(green, 0, 255);
      blue = constrain(blue, 0, 255);

      // write the values on the given pins
      analogWrite(redPin, red);
      analogWrite(greenPin, green);
      analogWrite(bluePin, blue);
    }
  }
}

// setPwmFrequency by Arduino Playground https://playground.arduino.cc/Code/PwmFrequency/
void setPwmFrequency(int pin, int divisor) {
  byte mode;
  if(pin == 5 || pin == 6 || pin == 9 || pin == 10) {
    switch(divisor) {
      case 1: mode = 0x01; break;
      case 8: mode = 0x02; break;
      case 64: mode = 0x03; break;
      case 256: mode = 0x04; break;
      case 1024: mode = 0x05; break;
      default: return;
    }
    if(pin == 5 || pin == 6) {
      TCCR0B = TCCR0B & 0b11111000 | mode;
    } else {
      TCCR1B = TCCR1B & 0b11111000 | mode;
    }
  } else if(pin == 3 || pin == 11) {
    switch(divisor) {
      case 1: mode = 0x01; break;
      case 8: mode = 0x02; break;
      case 32: mode = 0x03; break;
      case 64: mode = 0x04; break;
      case 128: mode = 0x05; break;
      case 256: mode = 0x06; break;
      case 1024: mode = 0x07; break;
      default: return;
    }
    TCCR2B = TCCR2B & 0b11111000 | mode;
  }
}
