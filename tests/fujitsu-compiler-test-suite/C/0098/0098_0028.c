#include <stdlib.h>
#include <math.h>
#include <stdio.h>

struct st1{
        float x[5][5][5];
} a,b,c,d;
int main(){
	long int k,j,i,pp;
	float s,t;
	for(k=0;k<5;k++){
		for(j=0;j<5;j++){
			for(i=0;i<5;i++){
				switch(k){
				case 0:
					a.x[i][j][k]=0.9;
					break;
				case 1:
					a.x[i][j][k]=2.1;
					break;
				case 2:
					a.x[i][j][k]=3.2;
					break;
				case 3:
					a.x[i][j][k]=4.1;
					break;
				case 4:
					a.x[i][j][k]=5.6;
				}
			}
		}
	}
	pp=0;
	for(k=0;k<5;k++){
		for(j=0;j<5;j++){
			for(i=0;i<5;i++){
				if(pp<5)
					b.x[i][j][k]=0.6;
				else if(pp<30)
					b.x[i][j][k]=1.1;
				else if(pp<55)
					b.x[i][j][k]=13.2;
				else if(pp<80)
					b.x[i][j][k]=4.21;
				else if(pp<105)
					b.x[i][j][k]=5.5;
					else
					b.x[i][j][k]=3.14;
				pp++;
			}
		}
	}
	pp=0;
	for(k=0;k<5;k++){
		for(j=0;j<5;j++){
			for(i=0;i<5;i++){
				if(pp<15)
					c.x[i][j][k]=0.6;
				else if(pp<40)
					c.x[i][j][k]=7.1;
				else if(pp<65)
					c.x[i][j][k]=12.2;
				else if(pp<90)
					c.x[i][j][k]=7.21;
				else if(pp<115)
					c.x[i][j][k]=1.5;
					else
					c.x[i][j][k]=23.14;
				pp++;
			}
		}
	}
	for(k=0;k<5;k++){
		for(j=0;j<5;j++){
			for(i=0;i<5;i++){
				d.x[i][j][k]=0.0;
			}
		}
	}
	s=0;
	for(k=0;k<5;k++){
		b.x[k][0][0]=0.0;
		for(i=0;i<5;i++){
			j=0;
			while(j<5){
				a.x[k][i][j]=b.x[k][i][j]+c.x[k][j][i];
				j++;
			}
		}
	}
	for(k=0;k<5;k++){
		for(i=0;i<5;i++){
			t=a.x[k][0][i];
LBL_22:
			;
			if ((j<=(-1)) || (j>4)) goto LBL_21;
			d.x[k][i][j]=b.x[k][j][i]/t;
			j--;
			goto LBL_22;
LBL_21:
			;
		}
		c.x[k][k][k]=d.x[k][k][k];
	}
	printf("  *****  *****\n");
	for(i=0;i<5;i++){
		for(j=0;j<5;j++){
			for(k=0;k<5;k++){
				printf(" a[%ld][%ld][%ld] => %f",i,j,k,a.x[i][j][k]);
				printf("     b[%ld][%ld][%ld] => %f\n",i,j,k,b.x[i][j][k]);
			}
		}
	}
	for(i=0;i<5;i++){
		for(j=0;j<5;j++){
			for(k=0;k<5;k++){
				printf(" c[%ld][%ld][%ld] => %f",i,j,k,c.x[i][j][k]);
				printf("     d[%ld][%ld][%ld] => %f\n",i,j,k,d.x[i][j][k]);
			}
		}
	}
	printf(" t => %f\n",t);
	exit (0);
}

