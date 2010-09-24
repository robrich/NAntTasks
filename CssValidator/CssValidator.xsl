<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html"/>
	<xsl:param name="applicationPath"/>

	<xsl:template match="/">
		<xsl:variable name="buildresults" select="//css-validator" />
		<xsl:choose>
			<xsl:when test="count($buildresults) > 0">
				<xsl:apply-templates select="$buildresults" />
			</xsl:when>
			<xsl:otherwise>
				<h2>Log does not contain any Xml output from CssValidator.</h2>
				<p>Please make sure that the task includes the attribute xmloutputfile=&quot;...&quot; and that cc.net merges the file</p>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="css-validator">
		<script type="text/javascript">
			function toggleDiv( imgId, divId ) {
				var eDiv = document.getElementById( divId );
				var eImg = document.getElementById( imgId );

				if ( eDiv.style.display == "none" ) {
					eDiv.style.display = "block";
					eImg.src = "<xsl:value-of select="$applicationPath"/>/images/arrow_minus_small.gif";
				} else {
					eDiv.style.display = "none";
					eImg.src = "<xsl:value-of select="$applicationPath"/>/images/arrow_plus_small.gif";
				}
			}
		</script>
		<h1>CSS Validation Results</h1>
		<div id="summary">
			<table>
				<tbody>
					<tr>
						<td>Results:</td>
						<td>
							<xsl:choose>
								<xsl:when test="@success = 'false'">
									<span class="error">Validation Failed</span>
								</xsl:when>
								<xsl:otherwise>
									<span>Validation Succeeded</span>
								</xsl:otherwise>
							</xsl:choose>
						</td>
					</tr>
					<tr>
						<td>Files:</td>
						<td><xsl:value-of select="count(file)"/></td>
					</tr>
					<tr>
						<td>Passes:</td>
						<td><xsl:value-of select="count(file[@errors = '0' and @warnings = '0'])"/></td>
					</tr>
					<tr>
						<td>Fails:</td>
						<td><xsl:value-of select="count(file[@errors != '0' or @warnings != '0'])"/></td>
					</tr>
				</tbody>
			</table>
		</div>
		<h3>Details:</h3>
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="file">
		<xsl:variable name="fileid" select="generate-id()" />
		<p>
			<xsl:choose>
				<xsl:when test="@errors > 0">
					<img src="{$applicationPath}/images/fxcop-critical-error.gif">
						<xsl:attribute name="title"><xsl:value-of select="@errors" /> Errors</xsl:attribute>
					</img>
				</xsl:when>
				<xsl:when test="@warnings > 0">
					<img src="{$applicationPath}/images/fxcop-error.gif">
						<xsl:attribute name="title"><xsl:value-of select="@warnings" /> Warnings</xsl:attribute>
					</img>
				</xsl:when>
				<xsl:otherwise>
					<img src="{$applicationPath}/images/check.jpg" width="16" height="16"/>
				</xsl:otherwise>
			</xsl:choose> 
			<input type="image" src="{$applicationPath}/images/arrow_plus_small.gif">
				<xsl:attribute name="id">img<xsl:value-of select="$fileid"/></xsl:attribute>
				<xsl:attribute name="onclick">javascript:toggleDiv('img<xsl:value-of select="$fileid"/>', 'detail<xsl:value-of select="$fileid"/>');</xsl:attribute>
			</input>
			<xsl:value-of select="@name"/>
		</p>
		<xsl:variable name="errors" select="error" />
		<div style="font-size: 0.9em; margin-left: 30px; border: 1px solid #cccccc; padding: 10px; display: none;">
			<xsl:attribute name="id">detail<xsl:value-of select="$fileid"/></xsl:attribute>
			Errors: <xsl:value-of select="@errors"/>,
			Warnings: <xsl:value-of select="@warnings"/>
			<xsl:if test="command != ''">
				<br />Command: <pre><xsl:value-of select="command"/></pre>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="count($errors) > 0">
					<ol>
						<xsl:apply-templates />
					</ol>
				</xsl:when>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="error">
		<li>
			<h3>
				<xsl:if test="@type='Error'">
					<xsl:attribute name="class">error</xsl:attribute>
				</xsl:if>
				<xsl:if test="@type='Warning'">
					<xsl:attribute name="class">warning</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@type"/> on line <xsl:value-of select="@line"/>
			</h3>
			<b>Description:</b> <xsl:value-of select="description"/><br />
			<b>Message:</b> <xsl:value-of select="message"/>
		</li>
	</xsl:template>

	<xsl:template match="stdout">
		<li style="list-style: none">
			<h3>StdOut:</h3>
			<pre><xsl:value-of select="current()"/></pre>
		</li>
	</xsl:template>

	<xsl:template match="command" />

</xsl:stylesheet>
