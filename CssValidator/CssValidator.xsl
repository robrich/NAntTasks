<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:output method="html"/>

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
		<xsl:choose>
			<xsl:when test="@success = 'false'">
				<h2>Validation Failed</h2>
			</xsl:when>
			<xsl:otherwise>
				<h2>Validation Succeeded</h2>
			</xsl:otherwise>
		</xsl:choose>
		<p>
			Total Errors: <xsl:value-of select="@totalErrors"/>, Total Warnings: <xsl:value-of select="@totalWarnings"/>
		</p>
		<xsl:apply-templates />
	</xsl:template>

	<xsl:template match="file">
		<hr/>
		<p>
			File: <b><xsl:value-of select="@name"/></b><br />
			Errors: <xsl:value-of select="@errors"/>,
			Warnings: <xsl:value-of select="@warnings"/>
			<xsl:if test="command != ''">
			<br />Command: <span style="font-size: 0.75em;"><xsl:value-of select="command"/></span>
			</xsl:if>

		</p>
		<xsl:variable name="errors" select="error" />
		<xsl:choose>
			<xsl:when test="count($errors) > 0">
				<div style="font-size: 0.9em; margin-left: 10px;">
					<ol>
						<xsl:apply-templates />
					</ol>
				</div>
			</xsl:when>
		</xsl:choose>
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
			<pre style="font-size: 0.75em;"><xsl:value-of select="current()"/></pre>
		</li>
	</xsl:template>

	<xsl:template match="command" />

</xsl:stylesheet>
