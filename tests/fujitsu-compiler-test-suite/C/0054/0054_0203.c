#include<stdio.h>
 

int main()  {

             char  c[4] = { 1,3,4,5 };
            short  s[4] = { 1,2,3,4 };
              int  i[4] = { 3,4,10,20 };
     unsigned int  ui[4] = { 1,2,5,10 };

   int a1,a2,a3,a4;
   int b1,b2,b3,b4;
   int x1,x2,x3,x4;
   int y1,y2,y3,y4;

  printf("21 TEST-START \n");

   switch(c[0])  {
     case (char)1:
       a1=10;
       break;
     case (char)2:
       a1=20;
       break;
     case (char)3:
       a1=30;
       break;
     case (char)5:
       a1=50;
       break;
     case (char)10:
       a1=100;
       break;
     default:
       a1=0;
  }

  if(a1==10)
    printf("21 1-1 TEST -O  K- \n");
  else
    printf("21 1-1 TEST - NG - \n");

  switch(c[1])  {
     case (short)1:
       a2=10;
       break;
     case (short)2:
       a2=20;
       break;
     case (short)3:
       a2=30;
       break;
     case (short)5:
       a2=50;
       break;
     case (short)10:
       a2=100;
       break;
     default:
       a2=0;
  }

  if(a2==30)
    printf("21 1-2 TEST -O  K- \n");
  else
    printf("21 1-2 TEST - NG - \n");

  switch(c[2])  {
     case 1:
       a3=10;
       break;
     case 2:
       a3=20;
       break;
     case 3:
       a3=30;
       break;
     case 5:
       a3=50;
       break;
     case 10:
       a3=100;
       break;
     default:
       a3=0;
  }

  if(a3==0)
    printf("21 1-3 TEST -O  K- \n");
  else
    printf("21 1-3 TEST - NG - \n");

  switch(c[3])  {
     case (unsigned int)1:
       a4=10;
       break;
     case (unsigned int)2:
       a4=20;
       break;
     case (unsigned int)3:
       a4=30;
       break;
     case (unsigned int)5:
       a4=50;
       break;
     case (unsigned int)10:
       a4=100;
       break;
     default:
       a4=0;
  }

  if(a4==50)
    printf("21 1-4 TEST -O  K- \n");
  else
    printf("21 1-4 TEST - NG - \n");

  switch(s[0])  {
     case (char)1:
       b1=10;
       break;
     case (char)2:
       b1=20;
       break;
     case (char)3:
       b1=30;
       break;
     case (char)5:
       b1=50;
       break;
     case (char)10:
       b1=100;
       break;
     default:
       b1=0;
  }

  if(b1==10)
    printf("21 2-1 TEST -O  K- \n");
  else
    printf("21 2-1 TEST - NG - \n");

  switch(s[1])  {
     case (short)1:
       b2=10;
       break;
     case (short)2:
       b2=20;
       break;
     case (short)3:
       b2=30;
       break;
     case (short)5:
       b2=50;
       break;
     case (short)10:
       b2=100;
       break;
     default:
       b2=0;
  }

  if(b2==20)
    printf("21 2-2 TEST -O  K- \n");
  else
    printf("21 2-2 TEST - NG - \n");

  switch(s[2])  {
     case 1:
       b3=10;
       break;
     case 2:
       b3=20;
       break;
     case 3:
       b3=30;
       break;
     case 5:
       b3=50;
       break;
     case 10:
       b3=100;
       break;
     default:
       b3=0;
  }

  if(b3==30)
    printf("21 2-3 TEST -O  K- \n");
  else
    printf("21 2-3 TEST - NG - \n");

  switch(s[3])  {
     case (unsigned int)1:
       b4=10;
       break;
     case (unsigned int)2:
       b4=20;
       break;
     case (unsigned int)3:
       b4=30;
       break;
     case (unsigned int)5:
       b4=50;
       break;
     case (unsigned int)10:
       b4=100;
       break;
     default:
       b4=0;
  }

  if(b4==0)
    printf("21 2-4 TEST -O  K- \n");
  else
    printf("21 2-4 TEST - NG - \n");

  switch(i[0]) {
     case (char)1:
       x1=10;
       break;
     case (char)2:
       x1=20;
       break;
     case (char)3:
       x1=30;
       break;
     case (char)5:
       x1=50;
       break;
     case (char)10:
       x1=100;
       break;
     default:
       x1=0;
  }

  if(x1==30)
    printf("21 3-1 TEST -O  K- \n");
  else
    printf("21 3-1 TEST - NG - \n");

  switch(i[1])  {
     case (short)1:
       x2=10;
       break;
     case (short)2:
       x2=20;
       break;
     case (short)3:
       x2=30;
       break;
     case (short)5:
       x2=50;
       break;
     case (short)10:
       x2=100;
       break;
     default:
       x2=0;
  }

  if(x2==0)
    printf("21 3-2 TEST -O  K- \n");
  else
    printf("21 3-2 TEST - NG - \n");

  switch(i[2])  {
     case 1:
       x3=10;
       break;
     case 2:
       x3=20;
       break;
     case 3:
       x3=30;
       break;
     case 5:
       x3=50;
       break;
     case 10:
       x3=100;
       break;
     default:
       x3=0;
  }

  if(x3==100)
    printf("21 3-3 TEST -O  K- \n");
  else
    printf("21 3-3 TEST - NG - \n");

  switch(i[3])  {
     case (unsigned int)1:
       x4=10;
       break;
     case (unsigned int)2:
       x4=20;
       break;
     case (unsigned int)3:
       x4=30;
       break;
     case (unsigned int)5:
       x4=50;
       break;
     case (unsigned int)10:
       x4=100;
       break;
     default:
       x4=0;
  }

  if(x4==0)
    printf("21 3-4 TEST -O  K- \n");
  else
    printf("21 3-4 TEST - NG - \n");

  switch(ui[0])  {
     case (char)1:
       y1=10;
       break;
     case (char)2:
       y1=20;
       break;
     case (char)3:
       y1=30;
       break;
     case (char)5:
       y1=50;
       break;
     case (char)10:
       y1=100;
       break;
     default:
       y1=0;
  }

  if(y1==10)
    printf("21 4-1 TEST -O  K- \n");
  else
    printf("21 4-1 TEST - NG - \n");

  switch(ui[1])  {
     case (short)1:
       y2=10;
       break;
     case (short)2:
       y2=20;
       break;
     case (short)3:
       y2=30;
       break;
     case (short)5:
       y2=50;
       break;
     case (short)10:
       y2=100;
       break;
     default:
       y2=0;
  }

  if(y2==20)
    printf("21 4-2 TEST -O  K- \n");
  else
    printf("21 4-2 TEST - NG - \n");

  switch(ui[2])  {
     case 1:
       y3=10;
       break;
     case 2:
       y3=20;
       break;
     case 3:
       y3=30;
       break;
     case 5:
       y3=50;
       break;
     case 10:
       y3=100;
       break;
     default:
       y3=0;
  }

  if(y3==50)
    printf("21 4-3 TEST -O  K- \n");
  else
    printf("21 4-3 TEST - NG - \n");

  switch(ui[3])  {
     case (unsigned int)1:
       y4=10;
       break;
     case (unsigned int)2:
       y4=20;
       break;
     case (unsigned int)3:
       y4=30;
       break;
     case (unsigned int)5:
       y4=50;
       break;
     case (unsigned int)10:
       y4=100;
       break;
     default:
       y4=0;
  }

  if(y4==100)
    printf("21 4-4 TEST -O  K- \n");
  else
    printf("21 4-4 TEST - NG - \n");

  printf("21 TEST-END \n");

}
