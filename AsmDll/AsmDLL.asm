.data
r dd 0.3f 
g dd 0.59f
b dd 0.11f
; Greyscale values for each channel based on human eye sensitivity

redWeight dq 0.3f 
greenWeight dq 0.59f
blueWeight dq 0.11f
rounding dq 0.5        

; Registers :
    ; - RCX: pointer to the red channel
    ; - RDX: pointer to the green channel
    ; - R8:  pointer to the blue channel
    ; - R9:  pointer to the output buffer

.code
EdgeDetect proc
    ; Red
    vmovups xmm0, xmmword ptr [rcx]           ; fetch 8 bytes of data (pixels in a group)
    vpmovzxbd ymm0, xmm0                      ; extend bytes to words (16-bit)
    vcvtdq2ps ymm0, ymm0                      ; convert integer data to floating point numbers

    ; Green
    vmovups xmm1, xmmword ptr [rdx]
    vpmovzxbd ymm1, xmm1
    vcvtdq2ps ymm1, ymm1

    ; Blue
    vmovups xmm2, xmmword ptr [r8]
    vpmovzxbd ymm2, xmm2
    vcvtdq2ps ymm2, ymm2

    ; Broadcast weights to all elements of the register
    vbroadcastss ymm3, r
    vbroadcastss ymm4, g
    vbroadcastss ymm5, b                   

    ; Multiply channel data by respective weights
    vmulps ymm0, ymm0, ymm3
    vmulps ymm1, ymm1, ymm4
    vmulps ymm2, ymm2, ymm5

    ; Sum the results to get grayscale value
    vaddps ymm0, ymm0, ymm1
    vaddps ymm0, ymm0, ymm2

    ; Convert results to integer values
    vcvttps2dq ymm0, ymm0

    ; store processed data to memory // xmm0 holds 16 bytes, that is 128 bits
    pextrb byte ptr [r9+0], xmm0, 0
    pextrb byte ptr [r9+1], xmm0, 4
    pextrb byte ptr [r9+2], xmm0, 8
    pextrb byte ptr [r9+3], xmm0, 12

    ; rearrange data in ymm0 register to handle the next pixels
    vpermq ymm0, ymm0, 11111110b              ; swap lower and upper parts of the register (at the 128-bit level) cause xmm is half the ymm

    ; store processed data for the next pixels
    pextrb byte ptr [r9+4], xmm0, 0
    pextrb byte ptr [r9+5], xmm0, 4
    pextrb byte ptr [r9+6], xmm0, 8
    pextrb byte ptr [r9+7], xmm0, 12
    
    ; End of procedure
    ret
EdgeDetect endp

    ; /////////////////////////////////////////////////////////////////////////////////////////

EdgeDetect2 proc
        ; rcx - pointer to input buffer
        ; rdx - pointer to output buffer
        ; r8 - width
        ; r9 - height

        ; Save registers
        push rbp
        push rbx
        push rsi
        push rdi
        push r12
        push r13
        push r14
        push r15

        ; Grayscale the input image
        call Grayscale

        ; Perform blur on the grayscale image
        ;call Blur

        ; Restore registers
        pop r15
        pop r14
        pop r13
        pop r12
        pop rdi
        pop rsi
        pop rbx
        pop rbp

        ret

Grayscale:
        ; Grayscale the input image
        ; Input: rcx = input buffer, rdx = output buffer, r8 = width, r9 = height
        ; Output: Grayscale image stored in the output buffer

        ; Calculate total number of pixels (width * height * 3)
        imul r9, r8
        imul r9, 3
        mov r13, r9

        xor rax, rax   ; rax = current pixel index

    processLoop:
        cmp rax, r13
        jge doneGrayscale

        ; Load RGB values
        movzx r10, byte ptr [rcx + rax]     ; R
        movzx r11, byte ptr [rcx + rax + 1] ; G
        movzx r12, byte ptr [rcx + rax + 2] ; B

        ; Convert to grayscale using weighted sum
        cvtsi2sd xmm0, r10                  ; R to float
        mulsd xmm0, qword ptr [redWeight]   ; R * 0.299

        cvtsi2sd xmm1, r11                  ; G to float
        mulsd xmm1, qword ptr [greenWeight] ; G * 0.587

        cvtsi2sd xmm2, r12                  ; B to float
        mulsd xmm2, qword ptr [blueWeight]  ; B * 0.114

        addsd xmm0, xmm1                    ; R + G
        addsd xmm0, xmm2                    ; R + G + B

        ; Round and convert back to integer
        addsd xmm0, qword ptr [rounding]    ; Add rounding value
        cvttsd2si r10d, xmm0                ; Convert to integer

        ; Store grayscale value in output buffer
        mov byte ptr [rdx + rax], r10b      ; R
        mov byte ptr [rdx + rax + 1], r10b  ; G
        mov byte ptr [rdx + rax + 2], r10b  ; B

        ; Move to next pixel
        add rax, 3
        jmp processLoop

    doneGrayscale:
        ret

        ; ///////////////////////////////////////
        ; Bluring the image
    
    Blur:
        ; Blur the grayscale image
        ; Input: rcx = input buffer, rdx = output buffer, r8 = width, r9 = height
        ; Output: Blurred image stored in the output buffer

        ; Calculate the number of bytes per row (stride)
        mov r10, r8
        imul r10, 3  ; r10 = width * 3 (bytes per row)

        ; Initialize loop counters
        mov r11, r9  ; r11 = height (outer loop counter)
        sub r11, 2   ; Avoid edge pixels

        mov r12, r8  ; r12 = width (inner loop counter)
        sub r12, 2   ; Avoid edge pixels

        ; Initialize pixel index
        xor rax, rax ; rax = current row index (in bytes)

    outerLoop:
        cmp rax, r11
        jge doneBlur

        ; Initialize inner loop counter
        xor rbx, rbx ; rbx = current column index (in bytes)

    innerLoop:
        cmp rbx, r12
        jge nextRow

        ; Calculate the sum of the 3x3 neighborhood
        xor r13, r13 ; r13 = sum of the neighborhood

        ; Top-left pixel
        mov r14, rax
        add r14, rbx
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Top-center pixel
        mov r14, rax
        add r14, rbx
        add r14, 3
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Top-right pixel
        mov r14, rax
        add r14, rbx
        add r14, 6
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Middle-left pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Middle-center pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, 3
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Middle-right pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, 6
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Bottom-left pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, r10
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Bottom-center pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, r10
        add r14, 3
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Bottom-right pixel
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, r10
        add r14, 6
        movzx r15, byte ptr [rcx + r14]
        add r13, r15

        ; Calculate the average (divide by 9)
        mov rax, r13
        mov rcx, 9
        xor rdx, rdx
        div rcx
        mov r13, rax

        ; Store the blurred pixel in the output buffer
        mov r14, rax
        add r14, rbx
        add r14, r10
        add r14, 3
        mov byte ptr [rdx + r14], r13b

        ; Move to the next column
        add rbx, 3
        jmp innerLoop

    nextRow:
        ; Move to the next row
        add rax, r10
        jmp outerLoop

    doneBlur:
        ret

EdgeDetect2 endp

end