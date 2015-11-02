parser grammar HorseshoeParser;
@parser::header {#pragma warning disable 3021}

options { tokenVocab=HorseshoeLexer; }

document
	:	(module | templateDecl)*
	;

ignoredContent 
   :  (DATA { System.String.IsNullOrWhiteSpace($DATA.text) }?<fail={ "Non-whitespace outside of template declaration." }>)+
   ;

body
	: DATA+
	;

module
   : ignoredContent? OPEN HASH MODULE name=qualifiedIdentifier CLOSE (templateDecl)* OPEN SLASH MODULE CLOSE ignoredContent?
   ;

templateDecl 
   : ignoredContent? OPEN invertTrim=TILDE? HASH TEMPLATE name=simpleIdentifier (COLON contextTypeName=typeName)? openTrimEnd=TILDE? CLOSE (expression)* OPEN closeTrimStart=TILDE? SLASH TEMPLATE  CLOSE ignoredContent?
   ;

expression  
   : substitution
   | unescapedSubstitution
	| conditional
   | listIteration
   | invoke
	| body
	;

conditional 
   : OPEN openTrimStart=TILDE? HASH IF NOT? id=scopeQualifiedIdentifier openTrimEnd=TILDE? CLOSE (expression)* elseClause? OPEN closeTrimStart=TILDE? SLASH IF closeTrimEnd=TILDE? CLOSE
	;

elseClause 
   : OPEN trimStart=TILDE? HASH ELSE trimEnd=TILDE? CLOSE (expression)*
   ;

listIteration
   : OPEN openTrimStart=TILDE? HASH FOREACH type=typeName variable=simpleIdentifier IN collection=scopeQualifiedIdentifier openTrimEnd=TILDE? CLOSE (expression)* OPEN closeTrimStart=TILDE? SLASH FOREACH closeTrimEnd=TILDE? CLOSE
   ;

invoke
   : OPEN trimStart=TILDE? HASH CALL method=qualifiedIdentifier LPAREN argument=scopeQualifiedIdentifier RPAREN trimEnd=TILDE? CLOSE   
   ;
   
substitution
	: OPEN trimStart=TILDE? id=scopeQualifiedIdentifier trimEnd=TILDE? CLOSE
	;

unescapedSubstitution
	: OPEN_UNESC trimStart=TILDE? id=scopeQualifiedIdentifier trimEnd=TILDE? CLOSE_UNESC
	;

scopeQualifiedIdentifier 
   : scope=TOPSCOPE? id=qualifiedIdentifier
   ;

qualifiedIdentifier
	: IDENTIFIER ('.' IDENTIFIER)*
	;

typeName 
   : qualifiedIdentifier ('<' typeName (',' typeName)* '>')? ('[' ']')*
   ;

simpleIdentifier
	: IDENTIFIER
	;
