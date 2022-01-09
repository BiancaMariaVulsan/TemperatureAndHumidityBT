#include <SoftwareSerial.h>
 
SoftwareSerial BT(2,3);


const int dataPin=8;
const int photoresistor = A0;
const int flameDetectorIn = A0;
const int flameDetectorDigitalIn = 5;
const int led = 13;
const int buzzerPin = 9;

int temperature = -1;
int humidity = -1;
int luminosity = -1;
int flameDigital = -1;
int flameAnalog = -1;

int index = 0;
char bytesToSend[20];

void setup()
{
  pinMode(photoresistor, INPUT);
  pinMode(flameDetectorDigitalIn, INPUT);
  pinMode(buzzerPin, OUTPUT);
  
  Serial.begin(9600);
  BT.begin(9600);
}

int readDHT11()
{
  uint8_t recvBuffer[5]; //1
  uint8_t cnt=7;
  uint8_t idx=0;
  for(int i=0; i<5; i++) //2
    recvBuffer[i]=0;

 //Start comunication with LOW pulse
 pinMode(dataPin, OUTPUT);
 digitalWrite(dataPin, LOW);
 delay(18);
 digitalWrite(dataPin, HIGH);

 delayMicroseconds(40); //3
 pinMode(dataPin, INPUT);

 pulseIn(dataPin, HIGH); //4

 //Read data packet
 unsigned int timeout = 10000; //loop iterations
 for(int i=0; i<40; i++) //5
 {
    timeout = 10000;
    while(digitalRead(dataPin)==LOW)
    {
      if(timeout ==0) return -1;
      timeout--;
    }

    unsigned long t = micros(); //6

    timeout = 10000;
    while(digitalRead(dataPin)==HIGH) //7
    {
      if(timeout ==0) return -1;
      timeout--;
    }

    if((micros()-t)>40) recvBuffer[idx]  |= (1<<cnt); //8
    if(cnt==0) // next byte?
    {
      cnt = 7; //restart at MSB
      idx++; //next byte!
    }
    else
    cnt--;
 }

 humidity = recvBuffer[0]; //% //9
 temperature = recvBuffer[2];
 uint8_t sum = recvBuffer[0]+recvBuffer[2];
 if(recvBuffer[4]!=sum)
 return -1; //10
 return 0;
}

void getDigits(int quantity)
{
  if(quantity/1000 != 0)// has 4 digits 
  {
    bytesToSend[index] = 4; index++;

    bytesToSend[index] = quantity/1000; index++;
    bytesToSend[index] = quantity/100%10; index++;
    bytesToSend[index] = quantity/10%10; index++;
    bytesToSend[index] = quantity%10; index++;
    
  } else if(quantity/100 != 0)// has 3 digits 
  {
    bytesToSend[index] = 3; index++;

    bytesToSend[index] = quantity/100; index++;
    bytesToSend[index] = quantity/10%10; index++;
    bytesToSend[index] = quantity%10; index++;
    
  } else if(quantity/10 != 0)// has 2 digits 
  {
    bytesToSend[index] = 2; index++;

    bytesToSend[index] = quantity/10; index++;
    bytesToSend[index] = quantity%10; index++;
    
  } else // just 1 digit
  {
    bytesToSend[index] = 1; index++;

    bytesToSend[index] = quantity; index++;
  }
}

void loop()
{
  // Get the termperature and humidity
  int ret = readDHT11();
//  if(ret!=0) Serial.println(ret);
//  Serial.print("Humidity: "); Serial.print(humidity); Serial.println(" %");
//  Serial.print("Temperature: "); Serial.print(temperature); Serial.println(" %");
  delay(2000); //ms

  // Get the luminosity
  luminosity = analogRead(photoresistor);
  // Serial.print("Luminosity: "); Serial.println(luminosity, DEC);

  // Get the flame
  flameDigital = digitalRead(flameDetectorDigitalIn);
  if(flameDigital == HIGH) // if flame is detected
  {
    digitalWrite(led, HIGH); // turn ON Arduino's LED
    digitalWrite(buzzerPin, HIGH);
  }
  else
  {
    digitalWrite(led, LOW); // turn OFF Arduino's LED
    digitalWrite(buzzerPin, LOW);
  }
  flameAnalog = analogRead(flameAnalog); 
  // Serial.print("Flame: "); Serial.println(flameAnalog); // print analog value to serial
  
  
    bytesToSend[0] = 36;
    bytesToSend[1] = temperature;
    bytesToSend[2] = humidity;
    index = 3;
    getDigits(luminosity);
    // bytesToSend[3] = luminosity;
    getDigits(flameAnalog);
    // bytesToSend[4] = flameAnalog;
//    Serial.println("Final index:");
//    Serial.println(index);  
    bytesToSend[index] = 70;
    BT.println(bytesToSend);
//    Serial.println("Bytes:");
    // Serial.println(bytesToSend);
}
