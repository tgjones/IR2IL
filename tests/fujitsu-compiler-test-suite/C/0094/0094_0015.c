#include  <stdio.h>
int main(){
  union s {
             signed int       b05:32;
           unsigned int       b06:32;
             signed short int b03:16;
           unsigned short int b04:16;
             signed char      b01:8;
           unsigned char      b02:8;
  };
  union s  x ;
  union s *y ;
  union s z[10];
  struct v {
    union s  u[10] ;
  } ;
  struct v  w;
  struct v *p;
  int      i;
  x.b01=2;
             if (x.b01==2) printf("**\n",x.b01);
             else          printf("**\n",x.b01);
             x.b01=0;
  x.b02=2;
             if (x.b02==2) printf("**\n",x.b02);
             else          printf("**\n",x.b02);
             x.b02=0;
  x.b03=2;
             if (x.b03==2) printf("**\n",x.b03);
             else          printf("**\n",x.b03);
             x.b03=0;
  x.b04=2;
             if (x.b04==2) printf("**\n",x.b04);
             else          printf("**\n",x.b04);
             x.b04=0;
  x.b05=1;
             if (x.b05==1) printf("**\n",x.b05);
             else          printf("**\n",x.b05);
             x.b05=0;
  x.b06=2;
             if (x.b06==2) printf("**\n",x.b06);
             else          printf("**\n",x.b06);
             x.b06=0;
             y=&x;
  y->b01=2;
             if (y->b01==2) printf("**\n",y->b01);
             else           printf("**\n",y->b01);
             y->b01=0;
  y->b02=2;
             if (y->b02==2) printf("**\n",y->b02);
             else           printf("**\n",y->b02);
             y->b02=0;
  y->b03=2;
             if (y->b03==2) printf("**\n",y->b03);
             else           printf("**\n",y->b03);
             y->b03=0;
  y->b04=2;
             if (y->b04==2) printf("**\n",y->b04);
             else           printf("**\n",y->b04);
             y->b04=0;
  y->b05=1;
             if (y->b05==1) printf("**\n",y->b05);
             else           printf("**\n",y->b05);
             y->b05=0;
  y->b06=2;
             if (y->b06==2) printf("**\n",y->b06);
             else           printf("**\n",y->b06);
             y->b06=0;
  for (i=0;i<10;i++)  z[i].b01=2;    
             if (z[i-1].b01==2) printf("**\n",z[i-1].b01);
             else               printf("**\n",z[i-1].b01);
             for (i=0;i<10;i++)  z[i].b01=0;    
  for (i=0;i<10;i++)  z[i].b02=2;    
             if (z[i-1].b02==2) printf("**\n",z[i-1].b02);
             else               printf("**\n",z[i-1].b02);
             for (i=0;i<10;i++)  z[i].b02=0;    
  for (i=0;i<10;i++)  z[i].b03=2;    
             if (z[i-1].b03==2) printf("**\n",z[i-1].b03);
             else               printf("**\n",z[i-1].b03);
             for (i=0;i<10;i++)  z[i].b01=0;    
  for (i=0;i<10;i++)  z[i].b04=2;    
             if (z[i-1].b04==2) printf("**\n",z[i-1].b04);
             else               printf("**\n",z[i-1].b04);
             for (i=0;i<10;i++)  z[i].b04=0;    
  for (i=0;i<10;i++)  z[i].b05=1;    
             if (z[i-1].b05==1) printf("**\n",z[i-1].b05);
             else               printf("**\n",z[i-1].b05);
             for (i=0;i<10;i++)  z[i].b05=0;    
  for (i=0;i<10;i++)  z[i].b06=2;    
             if (z[i-1].b06==2) printf("**\n",z[i-1].b06);
             else               printf("**\n",z[i-1].b06);
             for (i=0;i<10;i++)  z[i].b06=0;    
             p=&w;
  for (i=0;i<10;i++)  p->u[i].b01=2;
             if (p->u[i-1].b01==2) printf("**\n",p->u[i-1].b01);
             else                  printf("**\n",p->u[i-1].b01);
             for (i=0;i<10;i++)  p->u[i].b01=0;
  for (i=0;i<10;i++)  p->u[i].b02=2;
             if (p->u[i-1].b02==2) printf("**\n",p->u[i-1].b02);
             else                  printf("**\n",p->u[i-1].b02);
             for (i=0;i<10;i++)  p->u[i].b02=0;
  for (i=0;i<10;i++)  p->u[i].b03=2;
             if (p->u[i-1].b03==2) printf("**\n",p->u[i-1].b03);
             else                  printf("**\n",p->u[i-1].b03);
             for (i=0;i<10;i++)  p->u[i].b02=0;
  for (i=0;i<10;i++)  p->u[i].b04=2;
             if (p->u[i-1].b04==2) printf("**\n",p->u[i-1].b04);
             else                  printf("**\n",p->u[i-1].b04);
             for (i=0;i<10;i++)  p->u[i].b02=0;
  for (i=0;i<10;i++)  p->u[i].b05=1;
             if (p->u[i-1].b05==1) printf("**\n",p->u[i-1].b05);
             else                  printf("**\n",p->u[i-1].b05);
             for (i=0;i<10;i++)  p->u[i].b02=0;
  for (i=0;i<10;i++)  p->u[i].b06=2;
             if (p->u[i-1].b06==2) printf("**\n",p->u[i-1].b06);
             else                  printf("**\n",p->u[i-1].b06);
             for (i=0;i<10;i++)  p->u[i].b02=0;
}
