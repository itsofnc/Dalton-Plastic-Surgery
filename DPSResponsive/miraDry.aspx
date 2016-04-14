<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/mstSite.Master" CodeBehind="miraDry.aspx.vb" Inherits="DPSResponsive.miraDry" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="bodyContent" runat="server">
    <h1 class="title_data"><asp:Literal ID="title_data" runat="server"></asp:Literal></h1>
       
    <asp:Literal ID="body_data" runat="server" ></asp:Literal>
    
     <div class="col-md-12">
        <!-- 16:9 aspect ratio -->
        <div class="embed-responsive embed-responsive-16by9">
          <iframe width="560" height="315" src="https://www.youtube.com/embed/Zkg1Hj9bzDo?start=0&end=535&version=3" frameborder="0" allowfullscreen></iframe>
        </div>
    </div>
    
    <div class="col-md-12">
        <div class="panel panel-success">
          <!-- Default panel contents -->
          <div class="panel-heading">Patient FAQ</div>
          <!-- List group -->
          <ul class="list-group">
            <li class="list-group-item"><strong>What is the miraDry® treatment?</strong><br />
                miraDry is the only non-invasive, FDA cleared aesthetic treatment that safely removes underarm sweat and odor glands. It’s a quick in-office procedure that provides a permanent solution for underarm sweat.  
            </li>
            <li class="list-group-item"><strong>Don’t I need sweat glands?</strong><br />
                Not in your armpits! You are born with approximately 2 million sweat glands throughout your body. Your underarms only contain about 2% of those glands. With miraDry you will experience the many benefits of stopping sweat in the underarm area, but you will continue to sweat elsewhere. 
            </li>
            <li class="list-group-item"><strong>What is the procedure like?</strong><br />
                Most patients describe the procedure as painless with little downtime. Local anesthesia is administered to the underarms prior to the treatment to assist in increasing comfort. Some patients describe the initial anesthetic injections as the most painful part of the procedure. There are no incisions or cuts. Treatment is customized to each underarm with multiple placements of the miraDry handpiece. An office visit appointment will generally last about one hour.
            </li>
            <li class="list-group-item"><strong>How does it work?</strong><br />
                miraDry’s proprietary miraWave technology is the ideal wavelength for eliminating sweat and odor glands in the underarm. It non-invasively targets the area where the sweat glands reside and safely destroys them while simultaneously cooling your skin for added comfort.
            </li>
            <li class="list-group-item"><strong>What results can I expect?</strong><br />
                You can expect immediate and lasting results - sweat glands don’t come back after treatment. The amount of sweat reduction you desire should be discussed with your miraDry physician. Recent clinical information confirms an average of 82% sweat reduction in most patients. As with any aesthetic treatment results will vary by person.
            </li>
            <li class="list-group-item"><strong>How many treatments will I need?</strong><br />
                You can see results in as little as one treatment. However, as with any aesthetic treatment, your physician will determine the best protocol for your expected results.
            </li>
            <li class="list-group-item"><strong>Is the procedure painless?</strong><br />
                Since local anesthesia is administered to the underarm prior to treatment, most patients experience little to no pain. Patients rated treatment pain on an average score of 2 on a scale from 1 to 10 (1 = no pain, 10= worst pain).
            </li>
            <li class="list-group-item"><strong>Is there any downtime?</strong><br />                
                Most patients experience minimal downtime and return to regular activity (like returning to work) immediately. Exercise is typically resumed within several days. You may experience swelling, numbness, bruising and sensitivity in the underarm area for several days post-treatment.
            </li>
            <li class="list-group-item"><strong>Are there any side effects?</strong><br />                                
                The miraDry treatment has a strong safety record. Some localized soreness or swelling will occur, and typically clears within a few weeks. Some patients have short-term altered sensation in the skin of their underarms or upper arms, which gradually disappears. Got to <a href="www.miraDry.com" target="_blank">www.miraDry.com</a> for more information about the treatment.
            </li>
            <li class="list-group-item"><strong>How much does it cost?</strong><br />                                
                Pricing can vary depending on your specific treatment protocol and will be discussed during your consultation.
            </li>
          </ul>
        </div>
    </div>
    <div class="col-md-12">
        <hr />
       <center>Check out our <a href="miraPRLinks.aspx">Public Relations Links</a> about miraDry & miraSmooth.</center>
   </div>
</asp:Content>
