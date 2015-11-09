# Windows-Phone-8-Development
# NearMe - Ứng dụng đơn giản cho phép tìm kiếm các địa điểm xung quanh vị trí của mình trong một vùng nhất định
- Cài đặt: do lượng dữ liệu từ server chuyển về là rất lớn cho mỗi lần yêu cầu, trong khi bộ nhớ của điện thoại thì có giới 
hạn, vì thế giải quyết vấn đề này là hết sức cần thiết, bên cạnh đó vấn đề tiết kiệm băng thông cũng cần được lưu ý.
Để giải quyết vấn đề này, tôi đã làm như sau:
    + Thay vì load hết một lần toàn bộ dữ liệu thì nay tôi chia nhỏ lượng dữ liệu cần thiết làm nhiều phần, mỗi lần yêu cầu 
    một phần từ server.
    Trong ứng dụng này, mỗi lần ứng dụng sẽ load 20 item và hiển thị lên cho người dùng, nếu người dùng kéo xuống hết thì 
    tiếp tục load thêm 20 item nữa và cứ như vậy cho tới khi server đã trả hết kết quả hoặc người dùng không yêu cầu nữa.
    + Mỗi một địa điểm mà server trả về cho chúng ta có rất nhiều thông tin, trong đó có những thông tin không cần thiết
    và hầu hết là chiếm đa số, chỉ một số ít thông tin là chúng ta quan tâm như tên địa điểm, khoảng cách đến vị trí của chúng ta...
    Tại màn hình danh sách các địa điểm, chỉ hiển thị cho người dùng Tên địa điểm, ảnh đại diện và khoảng cách, vì thế
    trong kết quả nhận được từ server, tôi chỉ tách lấy những thông tin cần thiết đó và lưu lại, giảm được một lượng bộ nhớ đáng kể
    cho những thông tin không cần thiết.
    + Khi người dùng lựa chọn 1 địa điểm để xem chi tiết, ứng dụng sẽ yêu cầu từ server thông tin của địa điểm đó, và tất nhiên
    dữ liệu trả về là rất lớn với nhiều thông tin không cần thiết và ứng dụng chỉ lọc lấy những thông tin cần thiết như địa chỉ,
    thông tin liên lạc, trang web,...
    + Việc gửi yêu cầu nhiều lần, mỗi lần một ít có ưu điểm cũng như nhược điểm. Ưu điểm là khi người dùng chỉ thực hiện một
    số ít lần yêu cầu, lượng yêu cầu nhỏ sẽ giúp tiết kiệm băng thông, tuy nhiên khi người dùng không hài lòng với các kết quả
    trước đó và liên tục yêu cầu thêm kết quả thì lượng yêu cầu gửi đi và dữ liệu trả về càng ngày càng lớn, đó là nhược điểm
    của giải pháp này.
- Test ứng dụng:
  + Ứng dụng được test trên áy ảo và máy thật, sử dụng cả wifi và 3G.
  + Kết quả cho thấy, 
