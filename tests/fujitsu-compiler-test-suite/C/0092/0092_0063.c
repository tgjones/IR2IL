#include <stdio.h>

      float        r4=1.1e+5;  
      double       r8=2.2e+10;  
#ifdef NO_LONG_DOUBLE
      double  r16=3.3e+20;  
#else
      long double  r16=3.3e+20;  
#endif
int main( ) {
      float        ar4[10];  
      float        *pr4;  
  struct  a {
        float     r_ar4[10];  
  } *sr4;
      float        pr4_area[sizeof( float)];
      double       ar8[10];  
      double       *pr8;  
  struct  b {
        double    r_ar8[10];  
  } *sr8;
      double       pr8_area[sizeof( double )];
#ifdef NO_LONG_DOUBLE
      double  ar16[10];  
      double  *pr16;  
  struct  c {
        double  r_ar16[10];  
  } *sr16;
      double  pr16_area[sizeof(double)];
#else
      long double  ar16[10];  
      long double  *pr16;  
  struct  c {
        long double  r_ar16[10];  
  } *sr16;
      long double  pr16_area[sizeof( long double)];
#endif
  struct a              sr4_area;
  struct b              sr8_area;
  struct c              sr16_area;

  int      i;
  ar4[1]        = r4;
                     if (ar4[1]==1.1e+5 ) printf ("** (01)** OK\n");
                     else                 printf ("** (01)** NG ar4[1]=%e\n",ar4[1]);
                     pr4=pr4_area;
  *pr4          = r4;          
                     if (*pr4==1.1e+5 )   printf ("** (02)** OK\n");
                     else                 printf ("** (02)** NG *pr4=%e\n",pr4);
                     sr4=&sr4_area;
for (i=0;i<10;i++) 
  sr4->r_ar4[i] = r4;i=0;  
                     if (sr4->r_ar4[i]==1.1e+5 ) printf ("** (03)** OK\n");
                     else                        printf ("** (03)** NG sr4->r_ar4[i]=%e\n",sr4->r_ar4[i]);
  ar8[1]        = r8; 
                     if (ar8[1]==2.2e+10) printf ("** (04)** OK\n");
                     else                 printf ("** (04)** NG ar8[1]=%e\n",ar8[1]);
                     pr8=pr8_area;
  *pr8          =r8 ;         
                     if (*pr8==2.2e+10)   printf ("** (05)** OK\n");
                     else                 printf ("** (05)** NG *pr8=%e\n",*pr8);
                     sr8=&sr8_area;
for (i=0;i<10;i++) 
  sr8->r_ar8[i] = r8;i=0; 
                     if (sr8->r_ar8[i]==2.2e+10) printf ("** (06)** OK\n");
                     else                        printf ("** (06)** NG sr8->r_ar8[i]=%x\n",sr8->r_ar8[i]);
 ar16[1]        = r16;
                     if (ar16[1]==3.3e+20)   printf ("** (07)** OK\n");
                     else                    printf ("** (07)** NG ar16[1]=%e\n",ar16[1]);
                     pr16=pr16_area;
  *pr16          = r16;         
                     if (*pr16==3.3e+20)     printf ("** (08)** OK\n");
                     else                    printf ("** (08)** NG *pr16=%e\n",pr16);
                     sr16=&sr16_area;
for (i=0;i<10;i++) 
  sr16->r_ar16[i] = r16;i=0; 
                     if (sr16->r_ar16[i]==3.3e+20)  printf ("** (09)** OK\n");
                     else                           printf ("** (09)** NG sr16->r_ar16[i]=%e\n",sr16->r_ar16[i]);
}  