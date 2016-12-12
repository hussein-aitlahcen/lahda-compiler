/*******************************************************************************
 *      MSM -- Mini Stack Machine
 *
 * Copyright (c) 2004-2016  LIMSI / CNRS
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/
#include <ctype.h>
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define N (1 << 16)

#define swap(a, b, t) \
    do                \
    {                 \
	t tmp = a;    \
	a = b;        \
	b = tmp;      \
    } while (0)

static void error(int lno, const char *msg, ...)
{
    va_list args;
    if (lno != -1)
	fprintf(stderr, "error[%d]: ", lno);
    else
	fprintf(stderr, "error: ");
    va_start(args, msg);
    vfprintf(stderr, msg, args);
    va_end(args);
    fprintf(stderr, "\n");
    exit(EXIT_FAILURE);
}

typedef union {
    int i;
    float f;
} mem_t;
static mem_t mem[N], rs;
static int pc, sp, bp;

enum
{
    op_halt,
    op_drop,
    op_dup,
    op_pushi,
    op_pushf,
    op_get,
    op_set,
    op_read,
    op_write,
    op_addi,
    op_subi,
    op_muli,
    op_divi,
    op_modi,
    op_addf,
    op_subf,
    op_mulf,
    op_divf,
    op_not,
    op_and,
    op_or,
    op_itof,
    op_ftoi,
    op_cmpeqi,
    op_cmpnei,
    op_cmplti,
    op_cmplei,
    op_cmpgti,
    op_cmpgei,
    op_cmpeqf,
    op_cmpnef,
    op_cmpltf,
    op_cmplef,
    op_cmpgtf,
    op_cmpgef,
    op_jump,
    op_jumpt,
    op_jumpf,
    op_prep,
    op_call,
    op_ret,
    op_outi,
    op_outf,
    op_outc
};
struct
{
    char *name;
    int type;
} static const opd[] = {
    {"halt", 0},
    {"drop", 0},
    {"dup", 0},
    {"push.i", 1},
    {"push.f", 2},
    {"get", 1},
    {"set", 1},
    {"read", 0},
    {"write", 0},
    {"add.i", 0},
    {"sub.i", 0},
    {"mul.i", 0},
    {"div.i", 0},
    {"mod.i", 0},
    {"add.f", 0},
    {"sub.f", 0},
    {"mul.f", 0},
    {"div.f", 0},
    {"not", 0},
    {"and", 0},
    {"or", 0},
    {"itof", 0},
    {"ftoi", 0},
    {"cmpeq.i", 0},
    {"cmpne.i", 0},
    {"cmplt.i", 0},
    {"cmple.i", 0},
    {"cmpgt.i", 0},
    {"cmpge.i", 0},
    {"cmpeq.f", 0},
    {"cmpne.f", 0},
    {"cmplt.f", 0},
    {"cmple.f", 0},
    {"cmpgt.f", 0},
    {"cmpge.f", 0},
    {"jump", 3},
    {"jumpt", 3},
    {"jumpf", 3},
    {"prep", 3},
    {"call", 1},
    {"ret", 0},
    {"out.i", 0},
    {"out.f", 0},
    {"out.c", 0},
    {0, 0}};

typedef struct lbl_s lbl_t;
struct lbl_s
{
    lbl_t *next;
    char *name;
    int addr;
    int link;
};
static lbl_t *lbl = 0;

static lbl_t *label(const char *name)
{
    lbl_t *res = lbl;
    while (res)
    {
	if (!strcmp(res->name, name))
	    return res;
	res = res->next;
    }
    res = malloc(sizeof(lbl_t));
    res->name = malloc(strlen(name) + 1);
    strcpy(res->name, name);
    res->addr = res->link = -1;
    res->next = lbl;
    lbl = res;
    return res;
}

int main(int argc, char *argv[])
{
    int i, dbg = 0;
    FILE *file = stdin;
    argc--, argv++;
    while (argc > 0)
    {
	if (!strcmp(argv[0], "-d"))
	    dbg++;
	else if ((file = fopen(argv[0], "r")) == 0)
	    error(-1, "cannot open input file");
	argc--, argv++;
    }
    pc = 1;
    memset(mem, 0, sizeof(mem));
    do
    {
	int lno = 0;
	while (!feof(file))
	{
	    int cnt = 0, opc = -1;
	    char buf[4096], *tok[16], *ln = buf;
	    if (fgets(buf, sizeof(buf), file) == 0)
		break;
	    lno++;
	    while (*ln)
	    {
		while (*ln && isspace(*ln))
		    ln++;
		if (*ln == '\0' || *ln == ';')
		    break;
		tok[cnt++] = ln;
		while (*ln && !isspace(*ln))
		    ln++;
		if (*ln == '\0')
		    break;
		*ln++ = '\0';
	    }
	    if (cnt && tok[0][0] == '.')
	    {
		lbl_t *l = label(tok[0] + 1);
		if (l->addr != -1)
		    error(lno, "invalid label");
		l->addr = pc;
		for (i = 1; i < cnt; i++)
		    tok[i - 1] = tok[i];
		cnt--;
	    }
	    if (cnt == 0)
		continue;
	    for (i = 0; opc == -1 && opd[i].name; i++)
		if (!strcmp(tok[0], opd[i].name))
		    opc = i;
	    if (opc == -1)
		error(lno, "unknown opcode <%s>", tok[0]);
	    mem[pc++].i = opc;
	    if (opd[opc].type == 0)
	    {
		if (cnt != 1)
		    error(lno, "too much arguments");
	    }
	    else
	    {
		if (cnt != 2)
		    error(lno, "invalid arg count");
		if (opd[opc].type == 1)
		{
		    mem[pc++].i = atoi(tok[1]);
		}
		else if (opd[opc].type == 2)
		{
		    mem[pc++].f = atof(tok[1]);
		}
		else if (opd[opc].type == 3)
		{
		    lbl_t *l = label(tok[1]);
		    mem[pc++].i = l->link;
		    l->link = pc - 1;
		}
	    }
	}
    } while (0);
    mem[0].i = pc;
    do
    {
	lbl_t *l;
	for (l = lbl; l != 0; l = l->next)
	{
	    if (l->addr == -1)
		error(-1, "undefined label <%s>", l->name);
	    for (i = l->link; i != -1;)
	    {
		const int tmp = mem[i].i;
		mem[i].i = l->addr;
		i = tmp;
	    }
	}
    } while (0);
    if ((pc = label("start")->addr) < 0)
	error(-1, "start not defined");
    sp = N;
    bp = N;
    while (1)
    {
	const int tp = sp, nx = sp + 1;
	const int opc = mem[pc++].i;
	if (dbg)
	{
	    if (dbg > 1)
	    {
		printf("\nBP=%d\n", bp);
		for (i = N - 1; i >= sp; i--)
		    printf("  STK[%d] = %d\n", i, mem[i].i);
	    }
	    printf("  MEM[%d] %s", pc - 1, opd[opc].name);
	    switch (opd[opc].type)
	    {
	    case 1:
		printf(" %d", mem[pc].i);
		break;
	    case 2:
		printf(" %f", mem[pc].f);
		break;
	    case 3:
		printf(" %d", mem[pc].i);
		break;
	    }
	    printf("\n");
	}
	switch (opc)
	{
	case op_halt:
	    goto halt;
	case op_drop:
	    sp++;
	    break;
	case op_dup:
	    mem[--sp] = mem[tp];
	    break;
	case op_pushi:
	    mem[--sp].i = mem[pc++].i;
	    break;
	case op_pushf:
	    mem[--sp].f = mem[pc++].f;
	    break;
	case op_get:
	    mem[--sp] = mem[bp - mem[pc++].i - 1];
	    break;
	case op_set:
	    mem[bp - mem[pc++].i - 1] = mem[sp++];
	    break;
	case op_read:
	    mem[tp] = mem[mem[tp].i];
	    break;
	case op_write:
	    mem[mem[nx].i] = mem[tp];
	    sp += 2;
	    break;
	case op_addi:
	    mem[nx].i = mem[nx].i + mem[tp].i;
	    sp++;
	    break;
	case op_subi:
	    mem[nx].i = mem[nx].i - mem[tp].i;
	    sp++;
	    break;
	case op_muli:
	    mem[nx].i = mem[nx].i * mem[tp].i;
	    sp++;
	    break;
	case op_divi:
	    mem[nx].i = mem[nx].i / mem[tp].i;
	    sp++;
	    break;
	case op_modi:
	    mem[nx].i = mem[nx].i % mem[tp].i;
	    sp++;
	    break;
	case op_addf:
	    mem[nx].f = mem[nx].f + mem[tp].f;
	    sp++;
	    break;
	case op_subf:
	    mem[nx].f = mem[nx].f - mem[tp].f;
	    sp++;
	    break;
	case op_mulf:
	    mem[nx].f = mem[nx].f * mem[tp].f;
	    sp++;
	    break;
	case op_divf:
	    mem[nx].f = mem[nx].f / mem[tp].f;
	    sp++;
	    break;
	case op_not:
	    mem[tp].i = !mem[tp].i;
	    break;
	case op_and:
	    mem[nx].i = mem[nx].i && mem[tp].i;
	    sp++;
	    break;
	case op_or:
	    mem[nx].i = mem[nx].i || mem[tp].i;
	    sp++;
	    break;
	case op_itof:
	    mem[tp].f = mem[tp].i;
	    break;
	case op_ftoi:
	    mem[tp].i = mem[tp].f;
	    break;
	case op_cmpeqi:
	    mem[nx].i = mem[nx].i == mem[tp].i;
	    sp++;
	    break;
	case op_cmpnei:
	    mem[nx].i = mem[nx].i != mem[tp].i;
	    sp++;
	    break;
	case op_cmplti:
	    mem[nx].i = mem[nx].i < mem[tp].i;
	    sp++;
	    break;
	case op_cmplei:
	    mem[nx].i = mem[nx].i <= mem[tp].i;
	    sp++;
	    break;
	case op_cmpgti:
	    mem[nx].i = mem[nx].i > mem[tp].i;
	    sp++;
	    break;
	case op_cmpgei:
	    mem[nx].i = mem[nx].i >= mem[tp].i;
	    sp++;
	    break;
	case op_cmpeqf:
	    mem[nx].i = mem[nx].f == mem[tp].f;
	    sp++;
	    break;
	case op_cmpnef:
	    mem[nx].i = mem[nx].f != mem[tp].f;
	    sp++;
	    break;
	case op_cmpltf:
	    mem[nx].i = mem[nx].f < mem[tp].f;
	    sp++;
	    break;
	case op_cmplef:
	    mem[nx].i = mem[nx].f <= mem[tp].f;
	    sp++;
	    break;
	case op_cmpgtf:
	    mem[nx].i = mem[nx].f > mem[tp].f;
	    sp++;
	    break;
	case op_cmpgef:
	    mem[nx].i = mem[nx].f >= mem[tp].f;
	    sp++;
	    break;
	case op_jump:
	    pc = mem[pc].i;
	    break;
	case op_jumpt:
	    pc = (mem[sp++].i ? mem[pc].i : pc + 1);
	    break;
	case op_jumpf:
	    pc = (!mem[sp++].i ? mem[pc].i : pc + 1);
	    break;
	case op_prep:
	    mem[--sp].i = mem[pc++].i;
	    mem[--sp].i = bp;
	    break;
	case op_call:
	    bp = sp + mem[pc++].i;
	    swap(mem[bp + 1].i, pc, int);
	    break;
	case op_ret:
	    pc = mem[bp + 1].i;
	    mem[bp + 1] = mem[sp];
	    sp = bp;
	    bp = mem[sp++].i;
	    break;
	case op_outi:
	    printf("%d", mem[sp++].i);
	    break;
	case op_outf:
	    printf("%f", mem[sp++].f);
	    break;
	case op_outc:
	    printf("%c", mem[sp++].i);
	    break;
	}
    }
halt:
    return EXIT_SUCCESS;
}

/*******************************************************************************
 * This is the end...
 ******************************************************************************/
