int numButtons = 20;
int numPots = 5;
int buttonPins[20] = {11,12,13,14,15,16,17,25,26,28,29,30,31,39,40,41,32,33,34,35};

int pots[5][2] = {{0,1},{2,3},{5,4},{7,8},{9,10}};
bool up[5]    = {false, false, false, false, false};
bool noDir[5] = {true,  true,  true,  true,  true};
bool stp[5]   = {false, false, false, false, false};

#include <Bounce.h>

Bounce buttons[20] = {Bounce(11, 2),  //1
                      Bounce(12, 2),  //2
                      Bounce(13, 2),  //3
                      Bounce(14, 2),  //4
                      Bounce(15, 2),  //5
                      Bounce(16, 2),  //6
                      Bounce(17, 2),  //7
                      Bounce(25, 2),  //8
                      Bounce(26, 2),  //9
                      Bounce(28, 2),  //10
                      Bounce(29, 2),  //11
                      Bounce(30, 2),  //12
                      Bounce(31, 2),  //13
                      Bounce(39, 2),  //14
                      Bounce(40, 2),  //15
                      Bounce(41, 2),  //16
                      Bounce(32, 2),  //17
                      Bounce(33, 2),  //18
                      Bounce(34, 2),  //19
                      Bounce(35, 2)}; //20

#include <LiquidCrystal.h>
const int rs = 24, en = 23, d4 = 22, d5 = 21, d6 = 20, d7 = 19;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);

void setup() {
  lcd.begin(20, 4);
  Serial.begin(250000);
  Serial.println("Basic Encoder Test:");
  for(int i = 0; i < numPots; i++)
  {
    for(int n = 0; n < 2; n++)
    {
        pinMode(pots[i][n], INPUT_PULLUP);
    }
  }
  for(int i = 0; i < numButtons; i++)
  {
      pinMode(buttonPins[i], INPUT_PULLUP);
      digitalWrite(buttonPins[i], HIGH);
  }
}



void loop() {
  checkbuttons();
  checkpots();
  refreshLCD();
  LCDScroll();
  refreshLCDBtn();
}

int lastbtnval = 0;

void refreshLCDBtn()
{
  //idle 1023     -1
  //dn 1772       0
  //center 3242   26
  //right 5113    90
  //up 6953       180
  //left 9122     270
  
  int newval = analogRead(A0);
  if(lastbtnval != newval)
  {
    if(newval > 1100)
    {
      if(newval < 2000)
      {
        Joystick.hat(0);
      }
      else if (newval < 6000)
      {
        Joystick.hat(90);
      }
      else if (newval < 7000)
      {
        Joystick.hat(180);
      }
      else if (newval < 10000)
      {
        Joystick.hat(270);
      }
    }
    else
    {
      Joystick.hat(-1);
    }
    lastbtnval = newval;
  }
  
}

char rows[4][100];
int leng[4];
int pos[4];
int dir[4] = {1, 1, 1, 1};
double lasttime = millis();

void LCDScroll()
{
  double tim = millis();
  if(tim - lasttime > 1000)
  {
    lasttime = tim;
    for(int i = 0; i < 4; i++)
    {
      if(leng[i] > 21)
      {
        pos[i] += dir[i];
        if(pos[i] + 21 >= leng[i])
        {
          dir[i] = -1;
        } else if (pos[i] <= 0)
        {
          dir[i] = 1;
        }
        
        for(int n = 0; n < 20; n++)
        {
          lcd.setCursor(n, i);
          lcd.print(rows[i][n + pos[i]]);
        }
      }
    }
  }
}

void refreshLCD()
{
  int i = 0;
  int n = 0;
  while(Serial.available() > 0)
  {
    byte buff = Serial.read();
    if(i == 0)
    {
      n = buff-48;
      lcd.setCursor(0, n);
      lcd.print("                    ");
      dir[n] = 1;
      pos[n] = 0;
      leng[n] = 0;
    }
    else if(i < 21)
    {
      lcd.setCursor(i-1, n);
      lcd.print((char)buff);
    }
    if(i > 0)
    {
      rows[n][i-1] = (char)buff;
      leng[n]++;
    }
    i++;
  }
}

void checkbuttons()
{
  for(int i = 0; i < numButtons; i++)
  {
    buttons[i].update();  
    if (buttons[i].fallingEdge()) {
      Joystick.button(i+1, 1);
      Serial.print(i);
      Serial.println("dn");
    }
    if (buttons[i].risingEdge()) {
      Joystick.button(i+1, 0);
      Serial.print(i);
      Serial.println("up");
    }
  }
}

void checkpots()
{
  for(int i = 0; i < numPots; i++)
  {
    bool a = digitalRead(pots[i][0]);
    bool b = digitalRead(pots[i][1]);
    if(a == false && b == true && noDir[i] && !stp[i])
    {
      noDir[i] = false;
    }
    if(a == true && b == false && noDir[i] && !stp[i])
    {
      noDir[i] = false;
      up[i] = true;
    }
    if(a == false && b == false && !noDir[i] && !stp[i])
    {
      stp[i] = true;
      if(up[i])
      {
        Joystick.button(numButtons + i + 1, 1);
        Serial.print(i);
        Serial.println("up");
      }
      else
      {
        Joystick.button(numButtons + numPots + i + 1, 1);
        Serial.print(i);
        Serial.println("dn");
      }
    }
    if(a == true && b == true)
    {
      if(up[i])
      {
        Joystick.button(numButtons + i + 1, 0);
      }
      else
      {
        Joystick.button(numButtons + numPots + i + 1, 0);
      }
      up[i] = false;
      noDir[i] = true;
      stp[i] = false;
    }
  }
}


