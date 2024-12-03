#include <stdio.h>
#include <complex.h>

  float _Complex c8a[10],c8b[10],c8c[10],c8d[10],c8e[10],c8f;
  double _Complex c16a[10],c16b[10],c16c[10],c16d[10],c16e[10],c16f;
  float r4x[10],r4y[10],r4z,r4u[10],r4v[10],r4w[10];
  double r8x[10],r8y[10],r8z,r8u[10],r8v[10],r8w[10];
int main()
{
  int i,n;
  
  for(i=0;i<10;i++)
    {
      c8a[i] = 2;
      c8b[i] = (i+1)+(i+2)*I;
      c8c[i] = (i+1)*2+4*(i+1)*I;
      c8e[i] = 2+1.0I;
      c8f = 2;
      n = i-2;
      r4x[i] = -i;
      r4y[i] = 11-i;
      r4z = i+i*2*I;
    }

  for(i=0;i<10;i++)
    {
      c8d[i] = c8a[i];
      r4x[i] = c8d[i] + c8a[i];
      r4y[i] = c8d[i] - r4z;
    }

  for(i=0;i<10;i+=5)
    printf("(%f %f) (%f %f) (%f %f) (%f %f) (%f %f) \n",
           crealf(c8d[i]),cimagf(c8d[i]),
           crealf(c8d[i+1]),cimagf(c8d[i+1]),
           crealf(c8d[i+2]),cimagf(c8d[i+2]),
           crealf(c8d[i+3]),cimagf(c8d[i+3]),
           crealf(c8d[i+4]),cimagf(c8d[i+4]));
  printf("\n");
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4x[i],r4x[i+1],r4x[i+2],r4x[i+3],r4x[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4y[i],r4y[i+1],r4y[i+2],r4y[i+3],r4y[i+4]);
  
  for(i=0;i<10;i++)
    {
      c8b[i] = c8a[i]+2;
      r4y[i] = c8a[i]+3;
      r4x[i] = c8a[i]+4;
      r4u[i] = c8a[i];
      r4v[i] = c8a[i];
      r4w[i] = c8a[i];
    }
  for(i=0;i<10;i+=5)
    printf("(%f %f) (%f %f) (%f %f) (%f %f) (%f %f) \n",
           crealf(c8d[i]),cimagf(c8d[i]),
           crealf(c8d[i+1]),cimagf(c8d[i+1]),
           crealf(c8d[i+2]),cimagf(c8d[i+2]),
           crealf(c8d[i+3]),cimagf(c8d[i+3]),
           crealf(c8d[i+4]),cimagf(c8d[i+4]));
  printf("\n");
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4x[i],r4x[i+1],r4x[i+2],r4x[i+3],r4x[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4y[i],r4y[i+1],r4y[i+2],r4y[i+3],r4y[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4u[i],r4u[i+1],r4u[i+2],r4u[i+3],r4u[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4v[i],r4v[i+1],r4v[i+2],r4v[i+3],r4v[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r4w[i],r4w[i+1],r4w[i+2],r4w[i+3],r4w[i+4]);

  printf("\n");

  for(i=0;i<10;i++)
    {
      c16a[i] = 2;
      c16b[i] = (i+1)+(i+2)*I;
      c16c[i] = (i+1)*2+4*(i+1)*I;
      c16e[i] = 2+1.0I;
      c16f = 2;
      n = i-2;
      r8x[i] = -i;
      r8y[i] = 11-i;
      r8z = i+i*2*I;
    }

  for(i=0;i<10;i++)
    {
      c16d[i] = c16a[i];
      r8x[i] = c16d[i] + c16a[i];
      r8y[i] = c16d[i] - r8z;
    }

  for(i=0;i<10;i+=5)
    printf("(%f %f) (%f %f) (%f %f) (%f %f) (%f %f) \n",
           creal(c16d[i]),cimag(c16d[i]),
           creal(c16d[i+1]),cimag(c16d[i+1]),
           creal(c16d[i+2]),cimag(c16d[i+2]),
           creal(c16d[i+3]),cimag(c16d[i+3]),
           creal(c16d[i+4]),cimag(c16d[i+4]));
  printf("\n");
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8x[i],r8x[i+1],r8x[i+2],r8x[i+3],r8x[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8y[i],r8y[i+1],r8y[i+2],r8y[i+3],r8y[i+4]);
  
  for(i=0;i<10;i++)
    {
      c16b[i] = c16a[i]+2;
      r8y[i] = c16a[i]+3;
      r8x[i] = c16a[i]+4;
      r8u[i] = c16a[i];
      r8v[i] = c16a[i];
      r8w[i] = c16a[i];
    }
  for(i=0;i<10;i+=5)
    printf("(%f %f) (%f %f) (%f %f) (%f %f) (%f %f) \n",
           creal(c16d[i]),cimag(c16d[i]),
           creal(c16d[i+1]),cimag(c16d[i+1]),
           creal(c16d[i+2]),cimag(c16d[i+2]),
           creal(c16d[i+3]),cimag(c16d[i+3]),
           creal(c16d[i+4]),cimag(c16d[i+4]));
  printf("\n");
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8x[i],r8x[i+1],r8x[i+2],r8x[i+3],r8x[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8y[i],r8y[i+1],r8y[i+2],r8y[i+3],r8y[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8u[i],r8u[i+1],r8u[i+2],r8u[i+3],r8u[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8v[i],r8v[i+1],r8v[i+2],r8v[i+3],r8v[i+4]);
  for(i=0;i<10;i+=5)
    printf("%f %f %f %f %f \n",
	   r8w[i],r8w[i+1],r8w[i+2],r8w[i+3],r8w[i+4]);
}

