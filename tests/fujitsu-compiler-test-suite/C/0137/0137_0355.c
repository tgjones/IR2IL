#include <stdlib.h>
#include <math.h>
#include <stdio.h>
  struct t {
           signed   char      b01:1;
           unsigned char      b02:2;
           signed   short int b03:3;
           unsigned short int b04:4;
           signed   int       b05:5;
           unsigned int       b06:6;
  } x;
  struct s {
           signed   char      b01:8;
           unsigned char      b02:8;
           signed   short int b03:16;
           unsigned short int b04:16;
           signed   int       b05:32;
           unsigned int       b06:32;
  } y;
int main(){
  char      i1;
  short int i2;
  int       i4;
             x.b01=x.b02=x.b03=x.b04=x.b05=x.b06=1;
  i1=x.b01;  
             if (i1) printf("**dpmk2013-(01)** OK i1\n");
             else    printf("**dpmk2013-(01)** NG i1=%.1x\n",i1);
  i1=x.b02; 
             if (i1) printf("**dpmk2013-(02)** OK i1=%.1x\n",i1);
             else    printf("**dpmk2013-(02)** NG i1=%.1x\n",i1);
  i1=x.b03;
             if (i1) printf("**dpmk2013-(03)** OK i1=%.1x\n",i1);
             else    printf("**dpmk2013-(03)** NG i1=%.1x\n",i1);
  i1=x.b04;
             if (i1) printf("**dpmk2013-(04)** OK i1=%.1x\n",i1);
             else    printf("**dpmk2013-(04)** NG i1=%.1x\n",i1);
  i1=x.b05;  
             if (i1) printf("**dpmk2013-(05)** OK i1=%.1x\n",i1);
             else    printf("**dpmk2013-(05)** NG i1=%.1x\n",i1);
  i1=x.b06; 
             if (i1) printf("**dpmk2013-(06)** OK i1=%.1x\n",i1);
             else    printf("**dpmk2013-(06)** NG i1=%.1x\n",i1);
  i2=x.b01;  
             if (i2) printf("**dpmk2013-(07)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(07)** NG i2=%.4x\n",i2);
  i2=x.b02;
             if (i2) printf("**dpmk2013-(08)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(08)** NG i2=%.4x\n",i2);
  i2=x.b03;
             if (i2) printf("**dpmk2013-(09)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(09)** NG i2=%.4x\n",i2);
  i2=x.b04;
             if (i2) printf("**dpmk2013-(10)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(10)** NG i2=%.4x\n",i2);
  i2=x.b05;  
             if (i2) printf("**dpmk2013-(11)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(11)** NG i2=%.4x\n",i2);
  i2=x.b06; 
             if (i2) printf("**dpmk2013-(12)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(12)** NG i2=%.4x\n",i2);
  i4=x.b01;  
             if (i4) printf("**dpmk2013-(13)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(13)** NG i4=%.8x\n",i4);
  i4=x.b02;
             if (i4) printf("**dpmk2013-(14)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(14)** NG i4=%.8x\n",i4);
  i4=x.b03;
             if (i4) printf("**dpmk2013-(15)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(15)** NG i4=%.8x\n",i4);
  i4=x.b04;
             if (i4) printf("**dpmk2013-(16)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(16)** NG i4=%.8x\n",i4);
  i4=x.b05; 
             if (i4) printf("**dpmk2013-(17)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(17)** NG i4=%.8x\n",i4);
  i4=x.b06;  
             if (i4) printf("**dpmk2013-(18)** OK i4=%.8x\n",i4);
             else    printf("**dpmk2013-(18)** NG i4=%.8x\n",i4);
             y.b01=y.b02=y.b05=y.b06=0;
             y.b03=y.b04=1;
  i1=y.b01;  
             if (i1) printf("**dpmk2013-(19)** NG i1=%.1x\n",i1);
             else    printf("**dpmk2013-(19)** OK i1=%.1x\n",i1);
  i1=y.b02;  
             if (i1) printf("**dpmk2013-(20)** NG i1=%.1x\n",i1);
             else    printf("**dpmk2013-(20)** OK i1=%.1x\n",i1);
  i2=y.b03; 
             if (i2) printf("**dpmk2013-(21)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(21)** NG i2=%.4x\n",i2);
  i2=y.b04;  
             if (i2) printf("**dpmk2013-(22)** OK i2=%.4x\n",i2);
             else    printf("**dpmk2013-(22)** NG i2=%.4x\n",i2);
  i4=y.b05; 
             if (i4) printf("**dpmk2013-(23)** NG i4=%.8x\n",i4);
             else    printf("**dpmk2013-(23)** OK i4=%.8x\n",i4);
  i4=y.b06;  
             if (i4) printf("**dpmk2013-(24)** NG i4=%.8x\n",i4);
             else    printf("**dpmk2013-(24)** OK i4=%.8x\n",i4);
exit (0);
}
