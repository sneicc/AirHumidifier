#include <microDS3231.h>
MicroDS3231 rtc;

#include <Arduino.h>
#include "DHT_Async.h"

#define DHT_SENSOR_TYPE DHT_TYPE_22

static const int DHT_SENSOR_PIN = 7;
DHT_Async dht_sensor(DHT_SENSOR_PIN, DHT_SENSOR_TYPE);

#define CLOCK 11  //SH_CP
#define DATA 9    //DS
#define LATCH 10  //ST_CP
#define zero 0b11111100
#define one 0b01100000
#define two 0b11011010
#define three 0b11110010
#define four 0b01100110
#define five 0b10110110
#define six 0b10111110
#define seven 0b11100000
#define eight 0b11111110
#define nine 0b11110110
#define leftPartFi 0b11000110
#define rightPartFi 0b11001110
#define CSymbol 0b10011100
#define empty 0b00000000

#define EmptySymbol 13
#define CelsiusSymbol 12
#define LeftPartOfFI 11
#define RightPartOfFI 10


byte Digits[] = {zero, one, two, three, four, five, six, seven,
                    eight, nine, rightPartFi, leftPartFi, CSymbol, empty};

int Numbers[] = { EmptySymbol, EmptySymbol, EmptySymbol, EmptySymbol };
int mode = 0;

unsigned long CurrentTime, next_flick;
unsigned int to_flick = 4300;
byte digit = 0;

String InputString = "";
int ReceivedValue = 0;
bool IsNextMode = 0;
int ReceivedMode = 0;

unsigned long DHTMeasureDelay = 5000;
byte Hour = 0;
byte Minute = 0;
bool isNeedUpdateTime = true;


unsigned long DefualtTimeShowTime = 5000;
unsigned long DefualtHumidityShowTime = 5000;
unsigned long DefualtTemperatureShowTime = 5000;
unsigned long TimeShowTime = DefualtTimeShowTime;
unsigned long HumidityShowTime = DefualtHumidityShowTime;
unsigned long TemperatureShowTime = DefualtTemperatureShowTime;
unsigned long NextChangeMode;
bool isModeChanged = true;

bool isTimeActive = true;
bool isTemperatureActive = true;
bool isHumidityActive = true;

void setup() {
  //настраиваем контакты на выход
  pinMode(CLOCK, OUTPUT);
  pinMode(DATA, OUTPUT);
  pinMode(LATCH, OUTPUT);
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(13, OUTPUT);

  digitalWrite(LATCH, LOW);

  Serial.begin(9600);

  // проверка наличия модуля на линии i2c
  //вызов rtc.begin() не обязателен для работы
  while(!rtc.begin()){
    Serial.println("DS3231 not found");
  }

  //rtc.setTime(BUILD_SEC, BUILD_MIN, BUILD_HOUR, BUILD_DAY, BUILD_MONTH, BUILD_YEAR);
}

void DisplayDigit(int digit) {
  digitalWrite(LATCH, LOW);
  shiftOut(DATA, CLOCK, LSBFIRST, Digits[digit]);
  digitalWrite(LATCH, HIGH);
}

void DynamicIndication(byte digit) {
  switch (digit) {
    case 0:
      digitalWrite(5, LOW);
      DisplayDigit(Numbers[digit]);
      digitalWrite(2, HIGH);
      break;
    case 1:
      digitalWrite(2, LOW);
      DisplayDigit(Numbers[digit]);
      digitalWrite(3, HIGH);
      break;
    case 2:
      digitalWrite(3, LOW);
      DisplayDigit(Numbers[digit]);
      digitalWrite(4, HIGH);
      break;
    case 3:
      digitalWrite(4, LOW);
      DisplayDigit(Numbers[digit]);
      digitalWrite(5, HIGH);
      break;
  }
}

static bool MeasureEnvironment(float *temperature, float *humidity) 
{
    static unsigned long measurement_timestamp = millis();

    /* Measure once every four seconds. */
    if (millis() - measurement_timestamp > DHTMeasureDelay) 
    {
        if (dht_sensor.measure(temperature, humidity)) 
        {
            measurement_timestamp = millis();
            return (true);
        }
    }

    return (false);
}

void ReadSerial()
{
  if(Serial.available() > 0)
  {     
    char input = Serial.read();

    if(IsNextMode)
    {
      IsNextMode = 0;
      ReceivedMode = input - '0';
    }
    else if(input == 'M')
    {
      IsNextMode = 1;
      InputString = "";
    }
    else if(input == '\n')
    {
      mode = ReceivedMode;
    }
    else
    {
      InputString += input;     
    }  
  }
}

void TryChangeMode(unsigned long timeToShow)
{
  CurrentTime = millis(); 
  if(isModeChanged)
  {
    NextChangeMode = CurrentTime + timeToShow;
    isModeChanged = false;
  }

  if(CurrentTime >= NextChangeMode) 
  {
    ChangeMode();
  }  
}

void ChangeMode()
{
  mode++;
  if(mode >= 3) mode = 0;
  isModeChanged = true;
}

void SetTimeToShow(int hours, int minutes)
{
  Numbers[0] = (hours % 100) / 10;
  Numbers[1] = hours % 10;
  Numbers[2] = (minutes % 100) / 10;
  Numbers[3] = minutes % 10;
}

void SetTemperatureToShow(float temperature)
{
  Numbers[0] = EmptySymbol;
  Numbers[1] = CelsiusSymbol;
  Numbers[2] = ((int)temperature % 100) / 10;
  Numbers[3] = (int)temperature % 10;   
}

void SetHumidityToShow(float humidity)
{
  Numbers[0] = LeftPartOfFI;
  Numbers[1] = RightPartOfFI;
  Numbers[2] = ((int)humidity % 100) / 10;
  Numbers[3] = (int)humidity % 10;   
}

void SetEmptyToShow()
{
  Numbers[0] = EmptySymbol;
  Numbers[1] = EmptySymbol;
  Numbers[2] = EmptySymbol;
  Numbers[3] = EmptySymbol; 
}

void loop() {
  //unsigned long startTime = micros();
  int prevMode = mode;

  ReadSerial();

  if(isNeedUpdateTime)
  { 
    DateTime now = rtc.getTime();
    Hour = now.hour;
    Minute = now.minute;
  }
  
  float temperature;
  float humidity;
  bool isMeasured = MeasureEnvironment(&temperature, &humidity);
  isNeedUpdateTime = isMeasured;
  
  switch(mode) 
  {
    case 0:
      if(!isTimeActive) ChangeMode();
      else
      {
        SetTimeToShow(Hour, Minute);
        TryChangeMode(TimeShowTime);
        break;
      }

    case 1:
      if(!isTemperatureActive) ChangeMode();
      else
      {
        if (isMeasured || isModeChanged) 
        {
          SetTemperatureToShow(temperature);
        }     
        TryChangeMode(TemperatureShowTime);
        break;
      }

    case 2:
      if(!isHumidityActive)
      {
        ChangeMode();
        break;
      }
      else
      {
        if (isMeasured || isModeChanged) 
        {
          SetHumidityToShow(humidity);
        }
        TryChangeMode(HumidityShowTime);
        break;
      }

    case 3:
      mode = prevMode;

      ReceivedValue = InputString.toInt();
      to_flick = ReceivedValue;
      break;

    case 4:
      mode = prevMode;

      if(InputString.length() != 3) return;

      if(InputString[0] == '1') isTimeActive = 1;
      else isTimeActive = 0;
      if(InputString[1] == '1') isTemperatureActive = 1;
      else isTemperatureActive = 0;
      if(InputString[2] == '1') isHumidityActive = 1;
      else isHumidityActive = 0;

      if(!isTimeActive && !isTemperatureActive && !isHumidityActive) SetEmptyToShow();
      break;

    case 5:
      mode = prevMode;

      char type = InputString[0];
      bool setDefault = 0;
      if(InputString[1] == 'A') setDefault = 1;
      else
      {
        InputString.remove(0, 1);
        ReceivedValue = InputString.toInt() * 1000;
      }

      switch(type)
      {
        case 'T':
          if(setDefault) ReceivedValue = DefualtTimeShowTime;

          if(mode == 0) NextChangeMode += ReceivedValue - TimeShowTime;
          TimeShowTime = ReceivedValue;        
          break;
        case 'C':
          if(setDefault) ReceivedValue = DefualtTemperatureShowTime;

          if(mode == 1) NextChangeMode += ReceivedValue - TemperatureShowTime;
          TemperatureShowTime = ReceivedValue;
          break;
        case 'H':
          if(setDefault) ReceivedValue = DefualtHumidityShowTime;

          if(mode == 2) NextChangeMode += ReceivedValue - HumidityShowTime;
          HumidityShowTime = ReceivedValue;
          break;
      }     
  }


  CurrentTime = micros();
  if (CurrentTime > next_flick) {
    next_flick = CurrentTime + to_flick;
    digit++;
    if (digit == 4)
      digit = 0;
    DynamicIndication(digit);
  }

  //unsigned long endTime = micros();
  //Serial.println(endTime - startTime);  
}
