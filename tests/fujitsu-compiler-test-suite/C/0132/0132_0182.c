#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	double a[20]={
		2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,
		                 2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0	},
	b[20]={
		-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,
		                 -2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0	};
	union equ {
		double t;
		double u;
	} x;
	long int i;
	for(i=0;i<10;i++)
	{
		x.t = a[i+2] * b[i+1];
		a[i] = x.u * a[i];
	}
	for(i=0;i<20;i++) printf("a[%ld] => %12.5f\n",i,a[i]);
	exit (0);
}
