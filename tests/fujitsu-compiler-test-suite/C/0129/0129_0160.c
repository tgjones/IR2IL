#include <stdlib.h>
#include <math.h>
#include  <stdio.h>


int main(){
  struct t {
             signed long long int       b00:8;
             signed char      b01:6;
             signed short int b03:4;
             signed int       b05:2;
           unsigned long long int       b07:16;
           unsigned char      b02:5;
           unsigned short int b04:3;
           unsigned int       b06:2;
  } ;
  struct t  x ;
  struct t *y ;
  struct t  z[10] ;
  struct v {
    struct t u[10];
  } ;
  struct v  w;
  struct v *p;
  int      i;
  x.b05=x.b01=x.b02=x.b00=x.b03=x.b04=x.b07=x.b06=2; 
             if (x.b01==2) printf("**(01)** OK x.b01=%x\n",x.b01);
             else          printf("**(01)** NG x.b01=%x\n",x.b01);
             if (x.b02==2) printf("**(02)** OK x.b02=%x\n",x.b02);
             else          printf("**(02)** NG x.b02=%x\n",x.b02);
             if (x.b03==2) printf("**(03)** OK x.b03=%x\n",x.b03);
             else          printf("**(03)** NG x.b03=%x\n",x.b03);
             if (x.b04==2) printf("**(04)** OK x.b04=%x\n",x.b04);
             else          printf("**(04)** NG x.b04=%x\n",x.b04);
             if (x.b05==-2) printf("**(05)** OK x.b05=%x\n",x.b05);
             else          printf("**(05)** NG x.b05=%x\n",x.b05);
             if (x.b06==2) printf("**(06)** OK x.b06=%x\n",x.b06);
             else          printf("**(06)** NG x.b06=%x\n",x.b06);
             if (x.b00==2) printf("**(06a)** OK \n");
             else          printf("**(06a)** NG \n");
             if (x.b07==2) printf("**(06b)** OK \n");
             else          printf("**(06b)** NG \n");
             y=&x;
  y->b00=y->b05=y->b01=y->b07=y->b02=y->b03=y->b04=y->b06=2;
             if (y->b01==2) printf("**(07)** OK y->b01=%x\n",y->b01);
             else           printf("**(07)** NG y->b01=%x\n",y->b01);
             if (y->b02==2) printf("**(08)** OK y->b02=%x\n",y->b02);
             else           printf("**(08)** NG y->b02=%x\n",y->b02);
             if (y->b03==2) printf("**(09)** OK y->b03=%x\n",y->b03);
             else           printf("**(09)** NG y->b03=%x\n",y->b03);
             if (y->b04==2) printf("**(10)** OK y->b04=%x\n",y->b04);
             else           printf("**(10)** NG y->b04=%x\n",y->b04);
             if (y->b05==-2) printf("**(11)** OK y->b05=%x\n",y->b05);
             else           printf("**(11)** NG y->b05=%x\n",y->b05);
             if (y->b06==2) printf("**(12)** OK y->b06=%x\n",y->b06);
             else           printf("**(12)** NG y->b06=%x\n",y->b06);
             if (y->b00==-2) printf("**(12a)** OK \n");
             else          printf("**(12a)** NG \n");
             if (y->b07==2) printf("**(12b)** OK \n");
             else          printf("**(12b)** NG \n");
  for (i=0;i<10;i++)  z[i].b05    
                     =z[i].b01    
                     =z[i].b00    
                     =z[i].b02    
                     =z[i].b03    
                     =z[i].b04    
                     =z[i].b07    
                     =z[i].b06
                     =2;    
             if (z[i-1].b01==2) printf("**(13)** OK z[i-1].b01=%x\n",z[i-1].b01);
             else               printf("**(13)** NG z[i-1].b01=%x\n",z[i-1].b01);
             if (z[i-1].b02==2) printf("**(14)** OK z[i-1].b02=%x\n",z[i-1].b02);
             else               printf("**(14)** NG z[i-1].b02=%x\n",z[i-1].b02);
             if (z[i-1].b03==2) printf("**(15)** OK z[i-1].b03=%x\n",z[i-1].b03);
             else               printf("**(15)** NG z[i-1].b03=%x\n",z[i-1].b03);
             if (z[i-1].b04==2) printf("**(16)** OK z[i-1].b04=%x\n",z[i-1].b04);
             else               printf("**(16)** NG z[i-1].b04=%x\n",z[i-1].b04);
             if (z[i-1].b05==-2) printf("**(17)** OK z[i-1].b05=%x\n",z[i-1].b05);
             else               printf("**(17)** NG z[i-1].b05=%x\n",z[i-1].b05);
             if (z[i-1].b06==2) printf("**(18)** OK z[i-1].b06=%x\n",z[i-1].b06);
             else               printf("**(18)** NG z[i-1].b06=%x\n",z[i-1].b06);
 if (z[i-1].b00==2) printf("**(18a)** OK \n");
 else               printf("**(18a)** NG \n");
 if (z[i-1].b07==2) printf("**(18b)** OK \n");
 else               printf("**(18b)** NG \n");
             p=&w;
  for (i=0;i<10;i++)  p->u[i].b05    
                     =p->u[i].b01    
                     =p->u[i].b07    
                     =p->u[i].b02    
                     =p->u[i].b03    
                     =p->u[i].b04    
                     =p->u[i].b06
                     =p->u[i].b00    
                     =2;    
             if (p->u[i-1].b01==2) printf("**(19)** OK p->u[i-1].b01=%x\n",p->u[i-1].b01);
             else                  printf("**(19)** NG p->u[i-1].b01=%x\n",p->u[i-1].b01);
             if (p->u[i-1].b02==2) printf("**(20)** OK p->u[i-1].b02=%x\n",p->u[i-1].b02);
             else                  printf("**(20)** NG p->u[i-1].b02=%x\n",p->u[i-1].b02);
             if (p->u[i-1].b03==2) printf("**(21)** OK p->u[i-1].b03=%x\n",p->u[i-1].b03);
             else                  printf("**(21)** NG p->u[i-1].b03=%x\n",p->u[i-1].b03);
             if (p->u[i-1].b04==2) printf("**(22)** OK p->u[i-1].b04=%x\n",p->u[i-1].b04);
             else                  printf("**(22)** NG p->u[i-1].b04=%x\n",p->u[i-1].b04);
             if (p->u[i-1].b05==-2) printf("**(23)** OK p->u[i-1].b05=%x\n",p->u[i-1].b05);
             else                  printf("**(23)** NG p->u[i-1].b05=%x\n",p->u[i-1].b05);
             if (p->u[i-1].b06==2) printf("**(24)** OK p->u[i-1].b06=%x\n",p->u[i-1].b06);
             else                  printf("**(24)** NG p->u[i-1].b06=%x\n",p->u[i-1].b06);
 if (p->u[i-1].b00==2) printf("**(24a)** OK \n");
 else               printf("**(24a)** NG \n");
 if (p->u[i-1].b07==2) printf("**(24b)** OK \n");
 else               printf("**(24b)** NG \n");
exit (0);
}