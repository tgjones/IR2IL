#include <stdlib.h>

#include <stdio.h>
#include <math.h>
int main()
{
  float a[50]={1.,1.,1.,1.,1.,1.,1.,1.,1.,1.,
               2.,2.,2.,2.,2.,2.,2.,2.,2.,2.,
               3.,3.,3.,3.,3.,3.,3.,3.,3.,3.,
               4.,4.,4.,4.,4.,4.,4.,4.,4.,4.,
               5.,5.,5.,5.,5.,5.,5.,5.,5.,5.};
  float b[50]={5.,5.,5.,5.,5.,5.,5.,5.,5.,5.,
               4.,4.,4.,4.,4.,4.,4.,4.,4.,4.,
               3.,3.,3.,3.,3.,3.,3.,3.,3.,3.,
               2.,2.,2.,2.,2.,2.,2.,2.,2.,2.,
               1.,1.,1.,1.,1.,1.,1.,1.,1.,1.};
  float c[50];
  long int i;		     

  for ( i=1; i<49; i++ ){
    if (a[i]>3.0)
      {
	c[i]=a[i]+b[i+1];
	a[i]=b[i];
	if (c[i]<5.0)
	  {
	    b[i]=a[i-1]+5.0;
	    a[i]=a[i]+c[i];
	  }
	b[i+1]=a[i]+b[i];
      }
  }

  for ( i=0; i<50; i++ ) printf("a[%ld] = %f\n",i,a[i]);
  exit(0);
}