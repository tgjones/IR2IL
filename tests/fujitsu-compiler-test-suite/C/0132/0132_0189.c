#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	double a[20]={
		2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,
		                 -2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0	},
	b[20]={
		-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,-2.0,
		                 1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0	},
	x;
	long int i;
	x = 0.0;
	for(i=1;i<15;i++)
	{
		x += b[i];
		a[i+2] = a[i] + b[i];
	}
	for(i=0;i<20;i++)
		printf("a[%ld] => %12.5f\n",i,a[i]);
	printf("x => %12.5f\n",x);
	exit (0);
}
