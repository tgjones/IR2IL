#include <stdlib.h>
#include <stdio.h>
#include <math.h>

int main()
{
	double a[20]={
		2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,
		                 2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0,2.0	},
	b[20]={
		-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,
		                 -3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0,-3.0	},
	t;
	long int i;
	unsigned long int l1[10]={
		0,0,0,1,1,1,0,0,0,1	},
	l2[10]={
		0,1,1,1,1,1,0,0,0,1	};
	for(i=0;i<10;i++)
	{
		t = a[i];
		if (l1[i] == 1) {
			a[i+2] = -t;
			a[i] = t * b[i] - b[i+1] * a[i];
		}
		if (l2[i] == 1) {
			a[i] = t * b[i+1] - b[i+2] * a[i];
		}
	}
	for(i=0;i<20;i++)
		printf("a[%ld]=> %12.5f\n",i,a[i]);
	exit (0);
}
