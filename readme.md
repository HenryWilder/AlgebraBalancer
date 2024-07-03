# Shortcuts

## Notes

### Shortcuts

<table>
  <tr>
    <th>Input</th>
    <th>Action</th>
    <th>Examples</th>
  </tr>
  <tr>
    <td><kbd>Alt</kbd> + <kbd>Enter</kbd></td>
    <td>Toggle calculator pane</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>left</kbd></td>
    <td>Jump left one column</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>right</kbd></td>
    <td>Jump right one column</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Shift</kbd> + <kbd>Enter</kbd></td>
    <td>Duplicate line down</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>down</kbd></td>
    <td>Duplicate line down</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Alt</kbd> + <kbd>Shift</kbd> + <kbd>up</kbd></td>
    <td> Duplicate line up</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Ctrl</kbd> + (<kbd>1</kbd> or <kbd>2</kbd> or <kbd>3</kbd>)</td>
    <td>Insert selection into the corresponding calculator input</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Ctrl</kbd> + <kbd>Enter</kbd></td>
    <td>Clear calculator inputs and replace 1st with selection</td>
    <td></td>
  </tr>
  <tr>
    <td><kbd>Ctrl</kbd> + <kbd>space</kbd></td>
    <td>Calculate approximate value of the selection inline</td>
    <td>
      <mark>8(4 + 3)</mark><br/>
      <kbd>Ctrl</kbd> + <kbd>space</kbd><br/>
      8(4 + 3) = 56
    </td>
  </tr>
  <tr>
    <td><code>\<span style="color:dodgerblue">&lt;command&gt;</span>\</code></td>
    <td>
      
Unicode equivalent of the corresponding <code><span style="color:dodgerblue">&lt;command&gt;</span></code> $\LaTeX$ command
      
</td>
    <td>
      <code>\forall\</code> &forall;<br/>
      <code>\exists\</code> &exist;<br/>
      <code>\in\</code> &in;<br/>
      <code>\Omega\</code> &Omega;<br/>
      <code>\boxonbox\</code> ⧉
    </td>
  </tr>
  <tr>
    <td><code>\matrix<span style="color:dodgerblue">&lt;rows&gt;</span>x<span style="color:dodgerblue">&lt;cols&gt;</span></code></td>
    <td>Create a matrix with <code><span style="color:dodgerblue">&lt;rows&gt;</span></code> rows and <code><span style="color:dodgerblue">&lt;cols&gt;</span></code> columns (between 1 and 9)</td>
    <td><code>\matrix3x2</code><pre>
⎡& ... && ... &⎤
⎢& ... && ... &⎥
⎣& ... && ... &⎦
</pre>
    </td>
  </tr>
  <tr>
    <td><code>\det​<span style="color:dodgerblue">&lt;rows&gt;</span>x<span style="color:dodgerblue">&lt;cols&gt;</span></code></td>
    <td>Create a determinant with <code><span style="color:dodgerblue">&lt;rows&gt;</span></code> rows and <code><span style="color:dodgerblue">&lt;cols&gt;</span></code> columns (between 1 and 9)</td>
    <td><code>\det​3x2</code><pre>
⎢& ... && ... &⎥
⎢& ... && ... &⎥
⎢& ... && ... &⎥
</pre>
    </td>
  </tr>
  <tr>
    <td><code>\cases<span style="color:dodgerblue">&lt;cases&gt;</span></code></td>
    <td>Create a piecewise with <code><span style="color:dodgerblue">&lt;cases&gt;</span></code> cases (between 1 and 9)</td>
    <td><code>\cases3</code><pre>
⎧ & ... & if ...
⎨ & ... & if ...
⎩ & ... & if ...
</pre>
    </td>
  </tr>
  <tr>
    <td><code>\rcases<span style="color:dodgerblue">&lt;cases&gt;</span></code></td>
    <td>Create a reverse piecewise with <code><span style="color:dodgerblue">&lt;cases&gt;</span></code> cases (between 1 and 9)</td>
    <td><code>\rcases3</code><pre>
& ... & if ... & ⎫
& ... & if ... & ⎬
& ... & if ... & ⎭
</pre>
    </td>
  </tr>
  <tr>
    <td>
        <code>^<span style="color:dodgerblue">&lt;...&gt;</span></code><br/>
        or<br/>
        <code>^{<span style="color:dodgerblue">&lt;...&gt;</span>}</code>
    </td>
    <td>Superscript <code><span style="color:dodgerblue">&lt;...&gt;</span></code></td>
    <td>
      <code>^0-^9</code> ⁰-⁹<br/>
      <code>1^23</code> 1²3<br/>
      <code>^{0-9}</code> ⁰⁻⁹<br/>
      <code>1^{23}</code> 1²³
    </td>
  </tr>
  <tr>
    <td>
        <code>_<span style="color:dodgerblue">&lt;...&gt;</span></code><br/>
        or<br/>
        <code>_{<span style="color:dodgerblue">&lt;...&gt;</span>}</code>
    </td>
    <td>Subscript <code><span style="color:dodgerblue">&lt;...&gt;</span></code></td>
    <td>
      <code>_0-_9</code> ₀-₉<br/>
      <code>1_23</code> 1₂3<br/>
      <code>_{0-_9}</code> ₀₋₉<br/>
      <code>1_{23}</code> 1₂₃
    </td>
  </tr>
  <tr>
    <td>
        <code>$<span style="color:dodgerblue">&lt;...&gt;</span></code>
    </td>
    <td>Blackboard bold <code><span style="color:dodgerblue">&lt;...&gt;</span></code></td>
    <td>
      <code>$A-$Z</code> &Aopf;-&Zopf;<br/>
      <code>$a-$z</code> &aopf;-&zopf;<br/>
      <code>$0-$9</code> 𝟘-𝟡<br/>
    </td>
  </tr>
  <tr>
    <td>
        <code>\<span style="color:dodgerblue">&lt;...&gt;</span>\</code>
    </td>
    <td>Math variable <code><span style="color:dodgerblue">&lt;...&gt;</span></code></td>
    <td>
      <code>\A\-\Z\</code> 𝐴-𝑍<br/>
      <code>\a\-\z\</code> 𝑎-𝑧<br/>
    </td>
  </tr>
  <tr>
    <td><code>@@</code></td><td>Circ</td><td>∘</td>
  </tr>
  <tr>
    <td><code>@0</code></td><td>Degrees</td><td>°</td>
  </tr>
  <tr>
    <td><code>@*</code></td><td>Times</td><td>×</td>
  </tr>
  <tr>
    <td><code>@.</code></td><td>Cdot</td><td>⋅</td>
  </tr>
  <tr>
    <td><code>@/</code></td><td>Div</td><td>÷</td>
  </tr>
  <tr>
    <td><code>@-</code></td><td>Intersection</td><td>⋂</td>
  </tr>
  <tr>
    <td><code>@+</code></td><td>Union</td><td>⋃</td>
  </tr>
  <tr>
    <td><code>@2</code></td><td>Square root</td><td>√</td>
  </tr>
  <tr>
    <td><code>√³</code></td><td>Cube root</td><td>∛</td>
  </tr>
  <tr>
    <td><code>√⁴</code></td><td>Cube root</td><td>∜</td>
  </tr>
  <tr>
    <td><code>@8</code></td><td>Infinity</td><td>∞</td>
  </tr>
  <tr>
    <td><code>@6</code></td><td>Partial derivative</td><td>∂</td>
  </tr>
  <tr>
    <td><code>@A</code></td><td>Forall</td><td>∀</td>
  </tr>
  <tr>
    <td><code>@E</code></td><td>Exists</td><td>∃</td>
  </tr>
  <tr>
    <td><code>@v0</code></td><td>Varnothing</td><td>∅</td>
  </tr>
  <tr>
    <td><code>@I</code></td><td>Integral</td><td>∫</td>
  </tr>
  <tr><td><code>\&</code></td><td>Non-aligning ampersand</td><td>＆</td></tr>
</table>

### Brackets

`()`, `[]`, and `{}` are created in pairs and surround the selection.\
If the brackets are empty, backspacing the opening bracket deletes the closing bracket.\
`)`, `]`, and `}` can overtype each other (to make interval notation easier).

### Align

Use `&` to separate columns.\
Columns are aligned in an alternating pattern of right, left, right, left, etc.
<pre>
&longrightarrow; <span style="color:violet">&</span> &longleftarrow; <span style="color:violet">&</span> &longrightarrow; <span style="color:violet">&</span> &longleftarrow;
</pre>

Use `&&` to keep the same alignment direction in the new column.
<pre>
&longrightarrow; <span style="color:violet">&&</span> &longrightarrow; <span style="color:violet">&&</span> &longrightarrow; <span style="color:violet">&&</span> &longrightarrow; <span style="color:violet">&</span> &longleftarrow; <span style="color:violet">&&</span> &longleftarrow; <span style="color:violet">&&</span> &longleftarrow; <span style="color:violet">&&</span> &longleftarrow;
</pre>

Alignments are localized to the current "chunk". Chunks are separated by lines that have no `&`s.
<table>
  <tr>
    <th>Input</th>
    <th>Analysis</th>
    <th>Output</th>
  </tr>
  <tr>
    <td>
<pre>
apple & banana & orange & mango
000 & 000 & 000 & 000
blah blah blah blah
0000 & 0000 && 0000
0 & 0 && 0
</pre>
    </td><td>
<pre>
----------- chunk 1 -----------
apple & banana & orange & mango
  000 & 000    &    000 & 000
      ^        ^        ^
----------- chunk 2 -----------
blah blah blah blah<br/>
----------- chunk 3 -----------
0000 & 0000 && 0000
   0 & 0    && 0
     ^      ^^
-------------------------------
</pre>
    </td><td>
<pre>
apple & banana & orange & mango
  000 & 000    &    000 & 000
blah blah blah blah
0000 & 0000 && 0000
   0 & 0    && 0
</pre>
      </td>
    </tr>
</table>

## Calculator

Press enter to calculate.

## Symbol lookup

Press enter to search.\
Press escape to clear the search.\
Click a symbol to insert it in the notes section.
